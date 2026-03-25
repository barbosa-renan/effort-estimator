import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { EstimationRequest, EstimationResponse } from '../models/estimation.models';

interface ApiRequest {
  task_description?: string;
  technical_complexity: string;
  team_knowledge: string;
  external_integrations?: { count: number; complexity: string };
  external_dependencies?: { count: number; team_reliability: string };
}

interface ApiResponse {
  task_description?: string;
  optimistic: number;
  most_likely: number;
  pessimistic: number;
  pert_hours: number;
  standard_deviation: number;
  variance: number;
  story_points: number;
  confidence_range: { low: number; high: number };
  risk_level: string;
}

@Injectable({ providedIn: 'root' })
export class EstimationService {
  private readonly _http = inject(HttpClient);
  private readonly _estimationUrl = `${environment.apiBaseUrl}/api/estimation`;

  estimate(request: EstimationRequest): Observable<EstimationResponse> {
    return this._http
      .post<ApiResponse>(this._estimationUrl, this._toApiRequest(request))
      .pipe(map(this._fromApiResponse));
  }

  private _toApiRequest(request: EstimationRequest): ApiRequest {
    const payload: ApiRequest = {
      technical_complexity: request.technicalComplexity,
      team_knowledge: request.teamKnowledge,
    };

    if (request.taskDescription) {
      payload.task_description = request.taskDescription;
    }

    if (request.externalIntegrations) {
      payload.external_integrations = {
        count: request.externalIntegrations.count,
        complexity: request.externalIntegrations.complexity,
      };
    }

    if (request.externalDependencies) {
      payload.external_dependencies = {
        count: request.externalDependencies.count,
        team_reliability: request.externalDependencies.teamReliability,
      };
    }

    return payload;
  }

  private _fromApiResponse(api: ApiResponse): EstimationResponse {
    return {
      taskDescription: api.task_description,
      optimistic: api.optimistic,
      mostLikely: api.most_likely,
      pessimistic: api.pessimistic,
      pertHours: api.pert_hours,
      standardDeviation: api.standard_deviation,
      variance: api.variance,
      storyPoints: api.story_points,
      confidenceRange: api.confidence_range,
      riskLevel: api.risk_level as EstimationResponse['riskLevel'],
    };
  }
}
