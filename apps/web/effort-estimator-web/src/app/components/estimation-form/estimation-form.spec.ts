import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { EstimationFormComponent } from './estimation-form';
import { EstimationRequest } from '../../models/estimation.models';

describe('EstimationFormComponent', () => {
  let fixture: ComponentFixture<EstimationFormComponent>;
  let component: EstimationFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstimationFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EstimationFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('form_MissingRequiredFields_ShouldBeInvalid', () => {
    // Arrange
    // Act — form starts with empty required fields

    // Assert
    expect(component['form'].invalid).toBe(true);
  });

  it('form_RequiredFieldsFilled_ShouldBeValid', () => {
    // Arrange
    const form = component['form'];

    // Act
    form.patchValue({
      technicalComplexity: 'moderate',
      teamKnowledge: 'intermediate',
    });

    // Assert
    expect(form.valid).toBe(true);
  });

  it('buildRequest_IntegrationsCountZero_ShouldOmitIntegrationsFromPayload', () => {
    // Arrange
    const form = component['form'];
    form.patchValue({
      technicalComplexity: 'simple',
      teamKnowledge: 'expert',
      externalIntegrations: { count: 0, complexity: 'low' },
    });

    // Act
    let emittedRequest: EstimationRequest | undefined;
    component['submitted'].subscribe((req: EstimationRequest) => (emittedRequest = req));
    component['onSubmit']();

    // Assert
    expect(emittedRequest?.externalIntegrations).toBeUndefined();
  });

  it('buildRequest_IntegrationsCountGreaterThanZero_ShouldIncludeIntegrationsInPayload', () => {
    // Arrange
    const form = component['form'];
    form.patchValue({ technicalComplexity: 'complex', teamKnowledge: 'intermediate' });
    form.get('externalIntegrations.count')!.setValue(2);

    // Act
    let emittedRequest: EstimationRequest | undefined;
    component['submitted'].subscribe((req: EstimationRequest) => (emittedRequest = req));
    component['onSubmit']();

    // Assert
    expect(emittedRequest?.externalIntegrations).toBeDefined();
    expect(emittedRequest?.externalIntegrations?.count).toBe(2);
  });

  it('form_IntegrationsCountZero_ShouldDisableComplexityControl', () => {
    // Arrange
    const form = component['form'];
    form.get('externalIntegrations.count')!.setValue(0);

    // Act
    fixture.detectChanges();

    // Assert
    expect(form.get('externalIntegrations.complexity')!.disabled).toBe(true);
  });

  it('form_IntegrationsCountGreaterThanZero_ShouldEnableComplexityControl', () => {
    // Arrange
    const form = component['form'];

    // Act — setValue triggers valueChanges synchronously, enabling the control
    form.get('externalIntegrations.count')!.setValue(3);

    // Assert — check form model state directly without triggering change detection
    expect(form.get('externalIntegrations.complexity')!.disabled).toBe(false);
  });

  it('buildRequest_EmptyTaskDescription_ShouldOmitDescriptionFromPayload', () => {
    // Arrange
    const form = component['form'];
    form.patchValue({
      technicalComplexity: 'trivial',
      teamKnowledge: 'expert',
      taskDescription: '   ',
    });

    // Act
    let emittedRequest: EstimationRequest | undefined;
    component['submitted'].subscribe((req: EstimationRequest) => (emittedRequest = req));
    component['onSubmit']();

    // Assert
    expect(emittedRequest?.taskDescription).toBeUndefined();
  });
});
