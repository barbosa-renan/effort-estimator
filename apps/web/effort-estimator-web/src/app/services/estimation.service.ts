import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { EstimationRequest, EstimationResponse } from '../models/estimation.models';

@Injectable({ providedIn: 'root' })
export class EstimationService {
  private readonly _http = inject(HttpClient);
  private readonly _estimationUrl = `${environment.apiBaseUrl}/api/estimation`;

  estimate(request: EstimationRequest): Observable<EstimationResponse> {
    return this._http.post<EstimationResponse>(this._estimationUrl, request);
  }
}
