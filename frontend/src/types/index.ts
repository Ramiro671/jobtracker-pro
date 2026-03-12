export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface JobApplication {
  id: string;
  userId: string;
  title: string;
  companyId: string;
  jobUrl?: string;
  description?: string;
  status: ApplicationStatus;
  source: string;
  appliedAt: string;
  notes?: string;
}

export const ApplicationStatus = {
  Applied: 0,
  PhoneScreen: 1,
  Interview: 2,
  TechnicalTest: 3,
  FinalInterview: 4,
  OfferReceived: 5,
  OfferAccepted: 6,
  Rejected: 7,
  Withdrawn: 8,
} as const;

export type ApplicationStatus = typeof ApplicationStatus[keyof typeof ApplicationStatus];

export const STATUS_LABELS: Record<ApplicationStatus, string> = {
  [ApplicationStatus.Applied]: 'Applied',
  [ApplicationStatus.PhoneScreen]: 'Phone Screen',
  [ApplicationStatus.Interview]: 'Interview',
  [ApplicationStatus.TechnicalTest]: 'Technical Test',
  [ApplicationStatus.FinalInterview]: 'Final Interview',
  [ApplicationStatus.OfferReceived]: 'Offer Received',
  [ApplicationStatus.OfferAccepted]: 'Offer Accepted',
  [ApplicationStatus.Rejected]: 'Rejected',
  [ApplicationStatus.Withdrawn]: 'Withdrawn',
};

export const STATUS_COLORS: Record<ApplicationStatus, string> = {
  [ApplicationStatus.Applied]: 'bg-blue-100 text-blue-800',
  [ApplicationStatus.PhoneScreen]: 'bg-yellow-100 text-yellow-800',
  [ApplicationStatus.Interview]: 'bg-purple-100 text-purple-800',
  [ApplicationStatus.TechnicalTest]: 'bg-orange-100 text-orange-800',
  [ApplicationStatus.FinalInterview]: 'bg-indigo-100 text-indigo-800',
  [ApplicationStatus.OfferReceived]: 'bg-green-100 text-green-800',
  [ApplicationStatus.OfferAccepted]: 'bg-emerald-100 text-emerald-800',
  [ApplicationStatus.Rejected]: 'bg-red-100 text-red-800',
  [ApplicationStatus.Withdrawn]: 'bg-gray-100 text-gray-800',
};
