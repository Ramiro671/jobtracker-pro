import { type JobApplication, ApplicationStatus, STATUS_LABELS, STATUS_COLORS } from '../types';

interface Props {
  application: JobApplication;
  onDelete: (id: string) => void;
  onStatusChange: (id: string, status: ApplicationStatus) => void;
}

export default function JobApplicationCard({ application, onDelete, onStatusChange }: Props) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 hover:shadow-md transition">
      <div className="flex justify-between items-start mb-3">
        <div>
          <h3 className="font-semibold text-gray-900 text-base">{application.title}</h3>
          <p className="text-sm text-gray-500 mt-0.5">
            Applied {new Date(application.appliedAt).toLocaleDateString()}
          </p>
        </div>
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${STATUS_COLORS[application.status]}`}>
          {STATUS_LABELS[application.status]}
        </span>
      </div>

      {application.jobUrl && (
        <a
          href={application.jobUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-xs text-blue-600 hover:underline mb-3 block truncate"
        >
          {application.jobUrl}
        </a>
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
          onClick={() => onDelete(application.id)}
          className="text-xs text-red-500 hover:text-red-700 px-2 py-1.5 rounded-lg hover:bg-red-50 transition"
        >
          Delete
        </button>
      </div>
    </div>
  );
}
