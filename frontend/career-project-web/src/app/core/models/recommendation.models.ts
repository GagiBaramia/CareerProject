export interface RecommendationSkill {
  skillId: string;
  skillName: string;
  requiredLevel: string;
}

export interface JobRecommendation {
  jobId: string;
  title: string;
  description: string;
  employmentType: string;
  workFormat: string;
  location: string;
  salaryMin: number | null;
  salaryMax: number | null;
  salaryCurrency: string | null;
  companyId: string;
  companyName: string;
  score: number;
  skillOverlap: number;
  semanticSimilarity: number;
  requiredSkills: RecommendationSkill[];
}
