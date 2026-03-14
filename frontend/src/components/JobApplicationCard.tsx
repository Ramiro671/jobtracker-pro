import { useState } from 'react';
import { type JobApplication, ApplicationStatus, STATUS_LABELS, STATUS_COLORS } from '../types';

interface Props {
  application: JobApplication;
  onDelete: (id: string) => void;
  onStatusChange: (id: string, status: ApplicationStatus) => void;
  onEdit: (application: JobApplication) => void;
}

export default function JobApplicationCard({ application, onDelete, onStatusChange, onEdit }: Props) {
  const [confirmDelete, setConfirmDelete] = useState(false);

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 hover:shadow-md transition">
      <div className="flex justify-between items-start mb-1">
        <div className="flex-1 min-w-0 pr-2">
          <h3 className="font-semibold text-gray-900 text-base truncate">{application.title}</h3>
          <p className="text-sm text-gray-500 mt-0.5">{application.companyName}</p>
          <p className="text-xs text-gray-400 mt-0.5">
            Added {new Date(application.createdAt).toLocaleDateString()}
          </p>
        </div>
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full shrink-0 ${STATUS_COLORS[application.status]}`}>
          {STATUS_LABELS[application.status]}
        </span>
      </div>

      {application.jobUrl && (
        <a
          href={application.jobUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-xs text-blue-600 hover:underline mt-2 mb-1 block truncate"
        >
          {application.jobUrl}
        </a>
      )}

      {application.notes && (
        <p className="text-xs text-gray-500 mt-2 line-clamp-2">{application.notes}</p>
      )}

      <div className="flex items-center gap-2 mt-4">
        <select
          value={application.status}
          onChange={(e) => onStatusChange(application.id, Number(e.target.value) as ApplicationStatus)}
          className="flex-1 text-xs border border-gray-200 rounded-lg px-2 py-1.5 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          {Object.entries(STATUS_LABELS).map(([val, label]) => (
            <option key={val} value={val}>{label}</option>
          ))}
        </select>
        <button
          onClick={() => onEdit(application)}
          className="text-xs text-blue-500 hover:text-blue-700 px-2 py-1.5 rounded-lg hover:bg-blue-50 transition"
        >
          Edit
        </button>

        {confirmDelete ? (
          <>
            <button
              onClick={() => onDelete(application.id)}
              className="text-xs text-white bg-red-600 hover:bg-red-700 px-2 py-1.5 rounded-lg transition"
            >
              Confirm
            </button>
            <button
              onClick={() => setConfirmDelete(false)}
              className="text-xs text-gray-500 hover:text-gray-700 px-2 py-1.5 rounded-lg hover:bg-gray-100 transition"
            >
              Cancel
            </button>
          </>
        ) : (
          <button
            onClick={() => setConfirmDelete(true)}
            className="text-xs text-red-500 hover:text-red-700 px-2 py-1.5 rounded-lg hover:bg-red-50 transition"
          >
            Delete
          </button>
        )}
      </div>
    </div>
  );
}
