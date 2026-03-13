import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { jobApplicationsApi } from '../api/jobApplications';
import { type JobApplication, ApplicationStatus, STATUS_LABELS } from '../types';
import JobApplicationCard from '../components/JobApplicationCard';
import AddApplicationModal from '../components/AddApplicationModal';
import EditApplicationModal from '../components/EditApplicationModal';

const STALE_DAYS = 7;
const ACTIVE_STATUSES: ApplicationStatus[] = [
  ApplicationStatus.Applied,
  ApplicationStatus.PhoneScreen,
  ApplicationStatus.Interview,
  ApplicationStatus.TechnicalTest,
  ApplicationStatus.FinalInterview,
];

export default function DashboardPage() {
  const { userId, logout } = useAuth();
  const [applications, setApplications] = useState<JobApplication[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingApp, setEditingApp] = useState<JobApplication | null>(null);
  const [filter, setFilter] = useState<ApplicationStatus | 'all'>('all');
  const [search, setSearch] = useState('');

  const fetchApplications = async () => {
    if (!userId) return;
    try {
      const res = await jobApplicationsApi.getAll(userId);
      setApplications(res.data as JobApplication[]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchApplications(); }, []);

  const handleAdd = async (data: Parameters<typeof jobApplicationsApi.create>[0]) => {
    await jobApplicationsApi.create(data);
    await fetchApplications();
  };

  const handleEdit = async (id: string, data: { title: string; jobUrl?: string; notes?: string }) => {
    await jobApplicationsApi.edit(id, data);
    setApplications(prev => prev.map(a =>
      a.id === id ? { ...a, ...data, updatedAt: new Date().toISOString() } : a
    ));
  };

  const handleDelete = async (id: string) => {
    await jobApplicationsApi.delete(id);
    setApplications(prev => prev.filter(a => a.id !== id));
  };

  const handleStatusChange = async (id: string, status: ApplicationStatus) => {
    await jobApplicationsApi.updateStatus(id, status);
    setApplications(prev => prev.map(a => a.id === id ? { ...a, status, updatedAt: new Date().toISOString() } : a));
  };

  const staleApps = applications.filter(a => {
    if (!ACTIVE_STATUSES.includes(a.status)) return false;
    const lastActivity = new Date(a.updatedAt ?? a.createdAt);
    const daysSince = (Date.now() - lastActivity.getTime()) / (1000 * 60 * 60 * 24);
    return daysSince >= STALE_DAYS;
  });

  const filtered = applications
    .filter(a => filter === 'all' || a.status === filter)
    .filter(a => {
      if (!search.trim()) return true;
      const q = search.toLowerCase();
      return a.title.toLowerCase().includes(q) || a.companyName.toLowerCase().includes(q);
    });

  const stats = {
    total: applications.length,
    active: applications.filter(a =>
      a.status < ApplicationStatus.OfferAccepted &&
      a.status !== ApplicationStatus.Rejected &&
      a.status !== ApplicationStatus.Withdrawn
    ).length,
    offers: applications.filter(a =>
      a.status === ApplicationStatus.OfferReceived ||
      a.status === ApplicationStatus.OfferAccepted
    ).length,
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center">
        <h1 className="text-xl font-bold text-gray-900">JobTracker Pro</h1>
        <button onClick={logout} className="text-sm text-gray-500 hover:text-gray-700">
          Sign out
        </button>
      </header>

      <main className="max-w-5xl mx-auto px-6 py-8">
        <div className="grid grid-cols-3 gap-4 mb-8">
          {[
            { label: 'Total', value: stats.total, color: 'text-blue-600' },
            { label: 'Active', value: stats.active, color: 'text-purple-600' },
            { label: 'Offers', value: stats.offers, color: 'text-green-600' },
          ].map(s => (
            <div key={s.label} className="bg-white rounded-xl border border-gray-200 p-5">
              <p className="text-sm text-gray-500">{s.label}</p>
              <p className={`text-3xl font-bold mt-1 ${s.color}`}>{s.value}</p>
            </div>
          ))}
        </div>

        {staleApps.length > 0 && (
          <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 mb-6 flex items-start gap-3">
            <span className="text-amber-500 text-lg mt-0.5">&#9888;</span>
            <div>
              <p className="text-sm font-medium text-amber-800">
                {staleApps.length} application{staleApps.length > 1 ? 's have' : ' has'} had no activity for {STALE_DAYS}+ days:
              </p>
              <ul className="mt-1 space-y-0.5">
                {staleApps.map(a => (
                  <li key={a.id} className="text-xs text-amber-700">
                    {a.title} at {a.companyName} &mdash; {STATUS_LABELS[a.status]}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        )}

        <div className="flex flex-col sm:flex-row gap-3 justify-between items-start sm:items-center mb-5">
          <div className="flex gap-2 flex-1 w-full sm:w-auto">
            <input
              type="text"
              placeholder="Search by title or company..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="flex-1 border border-gray-200 rounded-lg px-3 py-2 text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <select
              value={filter}
              onChange={(e) => setFilter(e.target.value === 'all' ? 'all' : Number(e.target.value) as ApplicationStatus)}
              className="border border-gray-200 rounded-lg px-3 py-2 text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All statuses</option>
              {Object.entries(STATUS_LABELS).map(([val, label]) => (
                <option key={val} value={val}>{label}</option>
              ))}
            </select>
          </div>
          <button
            onClick={() => setShowModal(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition shrink-0"
          >
            + Add Application
          </button>
        </div>

        {loading ? (
          <p className="text-center text-gray-400 py-12">Loading...</p>
        ) : filtered.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <p className="text-lg mb-2">{search || filter !== 'all' ? 'No applications match your filters' : 'No applications yet'}</p>
            {!search && filter === 'all' && <p className="text-sm">Click &ldquo;+ Add Application&rdquo; to get started</p>}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {filtered.map(app => (
              <JobApplicationCard
                key={app.id}
                application={app}
                onDelete={handleDelete}
                onStatusChange={handleStatusChange}
                onEdit={setEditingApp}
              />
            ))}
          </div>
        )}
      </main>

      {showModal && userId && (
        <AddApplicationModal
          userId={userId}
          onAdd={handleAdd}
          onClose={() => setShowModal(false)}
        />
      )}

      {editingApp && (
        <EditApplicationModal
          application={editingApp}
          onSave={handleEdit}
          onClose={() => setEditingApp(null)}
        />
      )}
    </div>
  );
}
