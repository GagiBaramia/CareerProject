export type ProficiencyLevel = 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert';

export const PROFICIENCY_LEVELS: { value: ProficiencyLevel; label: string }[] = [
  { value: 'Beginner', label: 'დამწყები' },
  { value: 'Intermediate', label: 'საშუალო' },
  { value: 'Advanced', label: 'მაღალი' },
  { value: 'Expert', label: 'ექსპერტი' }
];

export function proficiencyLabel(level: ProficiencyLevel): string {
  return PROFICIENCY_LEVELS.find((l) => l.value === level)?.label ?? level;
}

export interface Skill {
  id: string;
  name: string;
}

export interface ProfileSkill {
  skillId: string;
  skillName: string;
  level: ProficiencyLevel;
}

export interface ProfileResponse {
  fullName: string;
  headline: string | null;
  cvSummary: string | null;
  location: string | null;
  photoUrl: string | null;
  skills: ProfileSkill[];
}

export interface UpdateProfileRequest {
  fullName: string;
  headline: string | null;
  cvSummary: string | null;
  location: string | null;
}

export interface UpdateProfileSkillsRequest {
  skills: { skillId: string; level: ProficiencyLevel }[];
}
