export type ApplicationStatus = 'Submitted' | 'InReview' | 'Interview' | 'Rejected' | 'Accepted';

export const APPLICATION_STATUSES: { value: ApplicationStatus; label: string }[] = [
  { value: 'Submitted', label: 'გაგზავნილია' },
  { value: 'InReview', label: 'განხილვაშია' },
  { value: 'Interview', label: 'გასაუბრება' },
  { value: 'Rejected', label: 'უარყოფილია' },
  { value: 'Accepted', label: 'მიღებულია' }
];

export function applicationStatusLabel(status: string): string {
  return APPLICATION_STATUSES.find((s) => s.value === status)?.label ?? status;
}

export interface JobApplication {
  id: string;
  jobId: string;
  jobTitle: string;
  companyName: string;
  personId: string;
  personName: string;
  personHeadline: string | null;
  personPhotoUrl: string | null;
  personSkills: string[];
  status: ApplicationStatus;
  appliedAt: string;
}
