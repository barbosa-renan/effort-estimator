import { Component, input, output, computed } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { EstimationResponse, RiskLevel } from '../../models/estimation.models';

const RiskLabelMap: Record<RiskLevel, string> = {
  Low: 'Baixo',
  Medium: 'Médio',
  High: 'Alto',
};

@Component({
  selector: 'app-estimation-result',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './estimation-result.html',
  styleUrl: './estimation-result.scss',
})
export class EstimationResultComponent {
  readonly result = input.required<EstimationResponse>();
  readonly reset = output<void>();

  protected readonly riskLabel = computed(() => RiskLabelMap[this.result().riskLevel]);

  protected readonly riskClass = computed(() => {
    const map: Record<RiskLevel, string> = {
      Low: 'risk-low',
      Medium: 'risk-medium',
      High: 'risk-high',
    };
    return map[this.result().riskLevel];
  });

  protected onReset(): void {
    this.reset.emit();
  }
}
