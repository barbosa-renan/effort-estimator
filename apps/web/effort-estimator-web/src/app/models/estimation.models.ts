export type TechnicalComplexity = 'trivial' | 'simple' | 'moderate' | 'complex' | 'very_complex';
export type TeamKnowledge = 'expert' | 'intermediate' | 'beginner' | 'unknown';
export type IntegrationComplexity = 'low' | 'medium' | 'high';
export type TeamReliability = 'high' | 'medium' | 'low';
export type RiskLevel = 'Low' | 'Medium' | 'High';

export interface ExternalIntegrationsRequest {
  count: number;
  complexity: IntegrationComplexity;
}

export interface ExternalDependenciesRequest {
  count: number;
  teamReliability: TeamReliability;
}

export interface EstimationRequest {
  taskDescription?: string;
  technicalComplexity: TechnicalComplexity;
  teamKnowledge: TeamKnowledge;
  externalIntegrations?: ExternalIntegrationsRequest;
  externalDependencies?: ExternalDependenciesRequest;
}

export interface ConfidenceRange {
  low: number;
  high: number;
}

export interface EstimationResponse {
  taskDescription: string;
  optimistic: number;
  mostLikely: number;
  pessimistic: number;
  pertHours: number;
  standardDeviation: number;
  variance: number;
  storyPoints: number;
  confidenceRange: ConfidenceRange;
  riskLevel: RiskLevel;
}
