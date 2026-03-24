import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { EstimationService } from './estimation.service';
import { EstimationRequest, EstimationResponse } from '../models/estimation.models';

describe('EstimationService', () => {
  let service: EstimationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EstimationService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(EstimationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('estimate_ValidRequest_ShouldPostToEstimationEndpoint', () => {
    // Arrange
    const request: EstimationRequest = {
      technicalComplexity: 'moderate',
      teamKnowledge: 'intermediate',
    };

    const mockResponse: EstimationResponse = {
      taskDescription: '',
      optimistic: 3,
      mostLikely: 8,
      pessimistic: 16,
      pertHours: 8.2,
      standardDeviation: 2.2,
      variance: 4.8,
      storyPoints: 5,
      confidenceRange: { low: 6.0, high: 10.4 },
      riskLevel: 'Low',
    };

    // Act
    let actualResponse: EstimationResponse | undefined;
    service.estimate(request).subscribe(res => (actualResponse = res));

    // Assert
    const req = httpMock.expectOne('http://localhost:5000/api/estimation');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);

    req.flush(mockResponse);
    expect(actualResponse).toEqual(mockResponse);
  });

  it('estimate_RequestWithIntegrations_ShouldIncludeIntegrationsInPayload', () => {
    // Arrange
    const request: EstimationRequest = {
      technicalComplexity: 'complex',
      teamKnowledge: 'beginner',
      externalIntegrations: { count: 3, complexity: 'high' },
    };

    // Act
    service.estimate(request).subscribe();

    // Assert
    const req = httpMock.expectOne('http://localhost:5000/api/estimation');
    expect(req.request.body.externalIntegrations).toEqual({ count: 3, complexity: 'high' });
    req.flush({});
  });
});
