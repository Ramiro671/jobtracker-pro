import { apiClient } from './client';
import { type ApplicationStatus } from '../types';

export const jobApplicationsApi = {
  getAll: (userId: string) =>
    apiClient.get(`/api/jobapplications/${userId}`),

  create: (data: {
    userId: string;
    title: string;
    companyName: string;
    jobUrl?: string;
    description?: string;
    source: string;
  }) => apiClient.post<{ id: string }>('/api/jobapplications', data),

  updateStatus: (id: string, newStatus: ApplicationStatus, notes?: string) =>
    apiClient.put(`/api/jobapplications/${id}`, { newStatus, notes }),

  delete: (id: string) =>
    apiClient.delete(`/api/jobapplications/${id}`),
};
