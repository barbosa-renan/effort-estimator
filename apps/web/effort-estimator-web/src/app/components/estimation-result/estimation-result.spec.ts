import { TestBed, ComponentFixture } from '@angular/core/testing';
import { EstimationResultComponent } from './estimation-result';
import { EstimationResponse } from '../../models/estimation.models';

function buildResponse(overrides: Partial<EstimationResponse> = {}): EstimationResponse {
  return {
    taskDescription: 'Test task',
    optimistic: 3,
    mostLikely: 8,
    pessimistic: 16,
    pertHours: 8.2,
    standardDeviation: 2.2,
    variance: 4.8,
    storyPoints: 5,
    confidenceRange: { low: 6.0, high: 10.4 },
    riskLevel: 'Low',
    ...overrides,
  };
}

describe('EstimationResultComponent', () => {
  let fixture: ComponentFixture<EstimationResultComponent>;
  let component: EstimationResultComponent;

  function create(response: EstimationResponse): void {
    fixture = TestBed.createComponent(EstimationResultComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('result', response);
    fixture.detectChanges();
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstimationResultComponent],
    }).compileComponents();
  });

  it('should create', () => {
    create(buildResponse());
    expect(component).toBeTruthy();
  });

  it('riskLabel_LowRisk_ShouldReturnBaixo', () => {
    // Arrange
    create(buildResponse({ riskLevel: 'Low' }));

    // Act
    const label = component['riskLabel']();

    // Assert
    expect(label).toBe('Baixo');
  });

  it('riskLabel_MediumRisk_ShouldReturnMedio', () => {
    // Arrange
    create(buildResponse({ riskLevel: 'Medium' }));

    // Act
    const label = component['riskLabel']();

    // Assert
    expect(label).toBe('Médio');
  });

  it('riskLabel_HighRisk_ShouldReturnAlto', () => {
    // Arrange
    create(buildResponse({ riskLevel: 'High' }));

    // Act
    const label = component['riskLabel']();

    // Assert
    expect(label).toBe('Alto');
  });

  it('riskClass_LowRisk_ShouldReturnRiskLowClass', () => {
    // Arrange
    create(buildResponse({ riskLevel: 'Low' }));

    // Act
    const cssClass = component['riskClass']();

    // Assert
    expect(cssClass).toBe('risk-low');
  });

  it('riskClass_HighRisk_ShouldReturnRiskHighClass', () => {
    // Arrange
    create(buildResponse({ riskLevel: 'High' }));

    // Act
    const cssClass = component['riskClass']();

    // Assert
    expect(cssClass).toBe('risk-high');
  });

  it('onReset_WhenCalled_ShouldEmitResetEvent', () => {
    // Arrange
    create(buildResponse());
    let resetEmitted = false;
    component['reset'].subscribe(() => (resetEmitted = true));

    // Act
    component['onReset']();

    // Assert
    expect(resetEmitted).toBe(true);
  });
});
