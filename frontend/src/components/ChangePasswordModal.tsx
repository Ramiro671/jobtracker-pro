import { useState } from 'react';
import { apiClient } from '../api/client';
import { useToast } from '../context/ToastContext';

interface Props { onClose: () => void; }

export default function ChangePasswordModal({ onClose }: Props) {
  const { showSuccess, showError } = useToast();
  const [form, setForm] = useState({ current: '', next: '', confirm: '' });
  const [loading, setLoading] = useState(false);
  const [validationError, setValidationError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError('');
    if (form.next !== form.confirm) {
      setValidationError('New passwords do not match.');
      return;
    }
    if (form.next.length < 8) {
      setValidationError('New password must be at least 8 characters.');
      return;
    }
    setLoading(true);
    try {
      await apiClient.put('/api/users/me/password', {
        currentPassword: form.current,
        newPassword: form.next,
      });
      showSuccess('Password updated successfully.');
      onClose();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { detail?: string } } })
        ?.response?.data?.detail ?? 'Failed to update password. Please try again.';
      showError(msg);
    } finally {
      setLoading(false);
    }
  };

  const inputCls = 'w-full border border-gray-300 dark:border-gray-600 rounded-lg px-3 py-2 text-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500';
  const labelCls = 'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1';

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-xl p-6 w-full max-w-sm mx-4">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">Change Password</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className={labelCls}>Current Password</label>
            <input type="password" required value={form.current}
              onChange={e => setForm({ ...form, current: e.target.value })}
              className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>New Password</label>
            <input type="password" required value={form.next}
              onChange={e => setForm({ ...form, next: e.target.value })}
              className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>Confirm New Password</label>
            <input type="password" required value={form.confirm}
              onChange={e => setForm({ ...form, confirm: e.target.value })}
              className={inputCls} />
          </div>
          {validationError && (
            <p className="text-sm text-red-600 dark:text-red-400">{validationError}</p>
          )}
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose}
              className="flex-1 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 py-2 rounded-lg text-sm hover:bg-gray-50 dark:hover:bg-gray-700 transition">
              Cancel
            </button>
            <button type="submit" disabled={loading}
              className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 transition disabled:opacity-50">
              {loading ? 'Updating...' : 'Update Password'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
