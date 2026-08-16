import { ProficiencyLevel } from './profile.models';

export interface JobSkill {
  skillId: string;
  skillName: string;
  requiredLevel: ProficiencyLevel;
}

export interface JobResponse {
  id: string;
  title: string;
  description: string;
  employmentType: string;
  workFormat: string;
  location: string;
  salaryMin: number | null;
  salaryMax: number | null;
  salaryCurrency: string | null;
  createdAt: string;
  companyId: string;
  companyName: string;
  requiredSkills: JobSkill[];
}

export interface CreateJobRequest {
  title: string;
  description: string;
  employmentType: string;
  workFormat: string;
  location: string;
  salaryMin: number | null;
  salaryMax: number | null;
  salaryCurrency: string | null;
  requiredSkills: { skillId: string; requiredLevel: ProficiencyLevel }[];
}

export const EMPLOYMENT_TYPES = [
  { value: 'FullTime', label: 'სრული განაკვეთი' },
  { value: 'PartTime', label: 'ნახევარი განაკვეთი' },
  { value: 'Contract', label: 'კონტრაქტი' },
  { value: 'Internship', label: 'სტაჟირება' }
];

export const WORK_FORMATS = [
  { value: 'OnSite', label: 'ოფისში' },
  { value: 'Remote', label: 'დისტანციური' },
  { value: 'Hybrid', label: 'ჰიბრიდული' }
];

export const CURRENCIES = ['USD', 'GEL', 'EUR'];
