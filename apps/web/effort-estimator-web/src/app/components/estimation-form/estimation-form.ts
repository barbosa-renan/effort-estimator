import { Component, output, signal, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { EstimationRequest, TechnicalComplexity, TeamKnowledge, IntegrationComplexity, TeamReliability } from '../../models/estimation.models';

@Component({
  selector: 'app-estimation-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './estimation-form.html',
  styleUrl: './estimation-form.scss',
})
export class EstimationFormComponent implements OnInit {
  readonly submitted = output<EstimationRequest>();

  protected readonly isIntegrationsOpen = signal(true);
  protected readonly isDependenciesOpen = signal(true);

  private readonly _fb = inject(FormBuilder);

  protected readonly form: FormGroup = this._fb.group({
    taskDescription: [''],
    technicalComplexity: ['', Validators.required],
    teamKnowledge: ['', Validators.required],
    externalIntegrations: this._fb.group({
      count: [0],
      complexity: [{ value: 'low', disabled: true }],
    }),
    externalDependencies: this._fb.group({
      count: [0],
      teamReliability: [{ value: 'medium', disabled: true }],
    }),
  });

  readonly complexityOptions: { value: TechnicalComplexity; label: string }[] = [
    { value: 'trivial', label: 'Trivial' },
    { value: 'simple', label: 'Simples' },
    { value: 'moderate', label: 'Moderada' },
    { value: 'complex', label: 'Complexa' },
    { value: 'very_complex', label: 'Muito Complexa' },
  ];

  readonly teamKnowledgeOptions: { value: TeamKnowledge; label: string }[] = [
    { value: 'expert', label: 'Especialista' },
    { value: 'intermediate', label: 'Intermediário' },
    { value: 'beginner', label: 'Iniciante' },
    { value: 'unknown', label: 'Desconhecido' },
  ];

  readonly integrationComplexityOptions: { value: IntegrationComplexity; label: string }[] = [
    { value: 'low', label: 'Baixa' },
    { value: 'medium', label: 'Média' },
    { value: 'high', label: 'Alta' },
  ];

  readonly teamReliabilityOptions: { value: TeamReliability; label: string }[] = [
    { value: 'high', label: 'Alta' },
    { value: 'medium', label: 'Média' },
    { value: 'low', label: 'Baixa' },
  ];

  ngOnInit(): void {
    this._watchIntegrationsCount();
    this._watchDependenciesCount();
  }

  private _watchIntegrationsCount(): void {
    const integrationsGroup = this.form.get('externalIntegrations') as FormGroup;
    const complexityControl = integrationsGroup.get('complexity')!;

    integrationsGroup.get('count')!.valueChanges.subscribe((count: number) => {
      if (count > 0) {
        complexityControl.enable();
      } else {
        complexityControl.disable();
        complexityControl.setValue('low');
      }
    });
  }

  private _watchDependenciesCount(): void {
    const dependenciesGroup = this.form.get('externalDependencies') as FormGroup;
    const reliabilityControl = dependenciesGroup.get('teamReliability')!;

    dependenciesGroup.get('count')!.valueChanges.subscribe((count: number) => {
      if (count > 0) {
        reliabilityControl.enable();
      } else {
        reliabilityControl.disable();
        reliabilityControl.setValue('medium');
      }
    });
  }

  protected toggleIntegrations(): void {
    this.isIntegrationsOpen.update(open => !open);
  }

  protected toggleDependencies(): void {
    this.isDependenciesOpen.update(open => !open);
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;

    const raw = this.form.getRawValue();
    const request = this._buildRequest(raw);
    this.submitted.emit(request);
  }

  private _buildRequest(raw: ReturnType<typeof this.form.getRawValue>): EstimationRequest {
    const request: EstimationRequest = {
      technicalComplexity: raw.technicalComplexity as TechnicalComplexity,
      teamKnowledge: raw.teamKnowledge as TeamKnowledge,
    };

    if (raw.taskDescription?.trim()) {
      request.taskDescription = raw.taskDescription.trim();
    }

    if (raw.externalIntegrations.count > 0) {
      request.externalIntegrations = {
        count: raw.externalIntegrations.count,
        complexity: raw.externalIntegrations.complexity as IntegrationComplexity,
      };
    }

    if (raw.externalDependencies.count > 0) {
      request.externalDependencies = {
        count: raw.externalDependencies.count,
        teamReliability: raw.externalDependencies.teamReliability as TeamReliability,
      };
    }

    return request;
  }

  protected get integrationsCount(): number {
    return this.form.get('externalIntegrations.count')?.value ?? 0;
  }

  protected get dependenciesCount(): number {
    return this.form.get('externalDependencies.count')?.value ?? 0;
  }
}
