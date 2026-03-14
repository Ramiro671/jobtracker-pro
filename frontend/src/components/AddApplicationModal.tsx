import { useState } from 'react';

interface Props {
  userId: string;
  onAdd: (data: {
    userId: string; title: string; companyName: string;
    jobUrl?: string; description?: string; source: string;
  }) => Promise<void>;
  onClose: () => void;
}

export default function AddApplicationModal({ userId, onAdd, onClose }: Props) {
  const [form, setForm] = useState({
    title: '', companyName: '', jobUrl: '', description: '', source: 'LinkedIn',
  });
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await onAdd({
        userId,
        ...form,
        jobUrl: form.jobUrl || undefined,
        description: form.description || undefined,
      });
      onClose();
    } finally {
      setLoading(false);
    }
  };

  const inputCls = 'w-full border border-gray-300 dark:border-gray-600 rounded-lg px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500';
  const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1';

  const field = (label: string, key: keyof typeof form, type = 'text', required = false) => (
    <div>
      <label className={labelCls}>{label}</label>
      <input
        type={type}
        value={form[key]}
        onChange={(e) => setForm({ ...form, [key]: e.target.value })}
        required={required}
        className={inputCls}
      />
    </div>
  );

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-xl p-6 w-full max-w-md mx-4">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">Add Application</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          {field('Job Title', 'title', 'text', true)}
          {field('Company', 'companyName', 'text', true)}
          {field('Job URL', 'jobUrl', 'url')}
          {field('Description', 'description')}
          <div>
            <label className={labelCls}>Source</label>
            <select
              value={form.source}
              onChange={(e) => setForm({ ...form, source: e.target.value })}
              className={inputCls}
            >
              {['LinkedIn', 'Indeed', 'Direct', 'Referral', 'Other'].map(s => (
                <option key={s}>{s}</option>
              ))}
            </select>
          </div>
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose}
              className="flex-1 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 py-2 rounded-lg text-sm hover:bg-gray-50 dark:hover:bg-gray-700 transition">
              Cancel
            </button>
            <button type="submit" disabled={loading}
              className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 transition disabled:opacity-50">
              {loading ? 'Adding...' : 'Add Application'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
