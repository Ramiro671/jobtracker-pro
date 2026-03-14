import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useTheme } from '../context/ThemeContext';
import { jobApplicationsApi } from '../api/jobApplications';
import { type JobApplication, ApplicationStatus, STATUS_LABELS } from '../types';
import JobApplicationCard from '../components/JobApplicationCard';
import AddApplicationModal from '../components/AddApplicationModal';
import EditApplicationModal from '../components/EditApplicationModal';
import ChangePasswordModal from '../components/ChangePasswordModal';

const STALE_DAYS = 7;
const PAGE_SIZE = 12;
const ACTIVE_STATUSES: ApplicationStatus[] = [
  ApplicationStatus.Applied,
  ApplicationStatus.PhoneScreen,
  ApplicationStatus.Interview,
  ApplicationStatus.TechnicalTest,
  ApplicationStatus.FinalInterview,
];

export default function DashboardPage() {
  const { userId, logout } = useAuth();
  const { showError, showSuccess } = useToast();
  const { theme, toggleTheme } = useTheme();
  const [applications, setApplications] = useState<JobApplication[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingApp, setEditingApp] = useState<JobApplication | null>(null);
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [filter, setFilter] = useState<ApplicationStatus | 'all'>('all');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const fetchApplications = async () => {
    if (!userId) return;
    try {
      const res = await jobApplicationsApi.getAll(userId);
      setApplications(res.data as JobApplication[]);
    } catch {
      showError('Failed to load applications. Please refresh.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchApplications(); }, []);
  useEffect(() => { setPage(1); }, [filter, search]);

  const handleAdd = async (data: Parameters<typeof jobApplicationsApi.create>[0]) => {
    try {
      await jobApplicationsApi.create(data);
      await fetchApplications();
      showSuccess('Application added.');
    } catch {
      showError('Failed to add application. Please try again.');
      throw new Error('create failed');
    }
  };

  const handleEdit = async (id: string, data: { title: string; companyName: string; jobUrl?: string; notes?: string }) => {
    try {
      await jobApplicationsApi.edit(id, data);
      setApplications(prev => prev.map(a =>
        a.id === id ? { ...a, ...data, updatedAt: new Date().toISOString() } : a
      ));
      showSuccess('Application updated.');
    } catch {
      showError('Failed to save changes. Please try again.');
      throw new Error('edit failed');
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await jobApplicationsApi.delete(id);
      setApplications(prev => prev.filter(a => a.id !== id));
      showSuccess('Application deleted.');
    } catch {
      showError('Failed to delete. Please try again.');
    }
  };

  const handleStatusChange = async (id: string, status: ApplicationStatus) => {
    try {
      await jobApplicationsApi.updateStatus(id, status);
      setApplications(prev => prev.map(a => a.id === id ? { ...a, status, updatedAt: new Date().toISOString() } : a));
      showSuccess('Status updated.');
    } catch {
      showError('Failed to update status. Please try again.');
    }
  };

  // ── CSV Export ──────────────────────────────────────────────
  const exportCSV = () => {
    const headers = ['Title', 'Company', 'Status', 'Source', 'Job URL', 'Notes', 'Added', 'Applied At'];
    const rows = applications.map(a => [
      a.title,
      a.companyName,
      STATUS_LABELS[a.status],
      a.source ?? '',
      a.jobUrl ?? '',
      (a.notes ?? '').replace(/\n/g, ' '),
      new Date(a.createdAt).toLocaleDateString(),
      a.appliedAt ? new Date(a.appliedAt).toLocaleDateString() : '',
    ]);
    const csv = [headers, ...rows]
      .map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `jobapplications-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  };

  // ── Stale detection ─────────────────────────────────────────
  const staleApps = applications.filter(a => {
    if (!ACTIVE_STATUSES.includes(a.status)) return false;
    const lastActivity = new Date(a.updatedAt ?? a.createdAt);
    const daysSince = (Date.now() - lastActivity.getTime()) / (1000 * 60 * 60 * 24);
    return daysSince >= STALE_DAYS;
  });

  // ── Filtering + pagination ──────────────────────────────────
  const filtered = applications
    .filter(a => filter === 'all' || a.status === filter)
    .filter(a => {
      if (!search.trim()) return true;
      const q = search.toLowerCase();
      return a.title.toLowerCase().includes(q) || a.companyName.toLowerCase().includes(q);
    });

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

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
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
      <header className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4 flex justify-between items-center">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">JobTracker Pro</h1>
        <div className="flex items-center gap-3">
          {/* Dark mode toggle */}
          <button
            onClick={toggleTheme}
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            className="text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition"
          >
            {theme === 'dark' ? '☀️' : '🌙'}
          </button>
          {/* Change password */}
          <button
            onClick={() => setShowChangePassword(true)}
            className="text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition"
          >
            Password
          </button>
          <button
            onClick={logout}
            className="text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition"
          >
            Sign out
          </button>
        </div>
      </header>

      <main className="max-w-5xl mx-auto px-6 py-8">
        {/* Stats */}
        <div className="grid grid-cols-3 gap-4 mb-8">
          {[
            { label: 'Total',  value: stats.total,  color: 'text-blue-600 dark:text-blue-400' },
            { label: 'Active', value: stats.active, color: 'text-purple-600 dark:text-purple-400' },
            { label: 'Offers', value: stats.offers, color: 'text-green-600 dark:text-green-400' },
          ].map(s => (
            <div key={s.label} className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-5">
              <p className="text-sm text-gray-500 dark:text-gray-400">{s.label}</p>
              <p className={`text-3xl font-bold mt-1 ${s.color}`}>{s.value}</p>
            </div>
          ))}
        </div>

        {/* Stale banner */}
        {staleApps.length > 0 && (
          <div className="bg-amber-50 dark:bg-amber-950 border border-amber-200 dark:border-amber-800 rounded-xl px-4 py-3 mb-6 flex items-start gap-3">
            <span className="text-amber-500 text-lg mt-0.5">&#9888;</span>
            <div>
              <p className="text-sm font-medium text-amber-800 dark:text-amber-300">
                {staleApps.length} application{staleApps.length > 1 ? 's have' : ' has'} had no activity for {STALE_DAYS}+ days:
              </p>
              <ul className="mt-1 space-y-0.5">
                {staleApps.map(a => (
                  <li key={a.id} className="text-xs text-amber-700 dark:text-amber-400">
                    {a.title} at {a.companyName} &mdash; {STATUS_LABELS[a.status]}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        )}

        {/* Controls */}
        <div className="flex flex-col sm:flex-row gap-3 justify-between items-start sm:items-center mb-5">
          <div className="flex gap-2 flex-1 w-full sm:w-auto">
            <input
              type="text"
              placeholder="Search by title or company..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="flex-1 border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <select
              value={filter}
              onChange={(e) => setFilter(e.target.value === 'all' ? 'all' : Number(e.target.value) as ApplicationStatus)}
              className="border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All statuses</option>
              {Object.entries(STATUS_LABELS).map(([val, label]) => (
                <option key={val} value={val}>{label}</option>
              ))}
            </select>
          </div>
          <div className="flex gap-2 shrink-0">
            {applications.length > 0 && (
              <button
                onClick={exportCSV}
                className="border border-gray-200 dark:border-gray-600 text-gray-600 dark:text-gray-300 text-sm font-medium px-3 py-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
              >
                ↓ CSV
              </button>
            )}
            <button
              onClick={() => setShowModal(true)}
              className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition"
            >
              + Add Application
            </button>
          </div>
        </div>

        {/* Cards */}
        {loading ? (
          <p className="text-center text-gray-400 dark:text-gray-500 py-12">Loading...</p>
        ) : filtered.length === 0 ? (
          <div className="text-center py-16 text-gray-400 dark:text-gray-500">
            <p className="text-lg mb-2">{search || filter !== 'all' ? 'No applications match your filters' : 'No applications yet'}</p>
            {!search && filter === 'all' && <p className="text-sm">Click &ldquo;+ Add Application&rdquo; to get started</p>}
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {paginated.map(app => (
                <JobApplicationCard
                  key={app.id}
                  application={app}
                  onDelete={handleDelete}
                  onStatusChange={handleStatusChange}
                  onEdit={setEditingApp}
                />
              ))}
            </div>

            {totalPages > 1 && (
              <div className="flex items-center justify-center gap-3 mt-8">
                <button
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="px-3 py-1.5 text-sm border border-gray-200 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-40 disabled:cursor-not-allowed transition"
                >
                  Previous
                </button>
                <span className="text-sm text-gray-500 dark:text-gray-400">
                  Page {page} of {totalPages} &middot; {filtered.length} results
                </span>
                <button
                  onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="px-3 py-1.5 text-sm border border-gray-200 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-40 disabled:cursor-not-allowed transition"
                >
                  Next
                </button>
              </div>
            )}
          </>
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

      {showChangePassword && userId && (
        <ChangePasswordModal
          onClose={() => setShowChangePassword(false)}
        />
      )}
    </div>
  );
}
