import { Component, signal, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { EstimationFormComponent } from '../../components/estimation-form/estimation-form';
import { EstimationResultComponent } from '../../components/estimation-result/estimation-result';
import { EstimationService } from '../../services/estimation.service';
import { EstimationRequest, EstimationResponse } from '../../models/estimation.models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [EstimationFormComponent, EstimationResultComponent],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class HomePage {
  private readonly _estimationService = inject(EstimationService);

  protected readonly isLoading = signal(false);
  protected readonly result = signal<EstimationResponse | null>(null);
  protected readonly errorMessage = signal<string | null>(null);

  protected onFormSubmitted(request: EstimationRequest): void {
    this.isLoading.set(true);
    this.result.set(null);
    this.errorMessage.set(null);

    this._estimationService.estimate(request).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.result.set(response);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(this._resolveErrorMessage(err));
      },
    });
  }

  protected onReset(): void {
    this.result.set(null);
    this.errorMessage.set(null);
  }

  private _resolveErrorMessage(err: HttpErrorResponse): string {
    if (err.status === 0) {
      return 'Não foi possível conectar à API. Verifique se o servidor está em execução.';
    }
    if (err.status === 400) {
      return 'Os dados informados são inválidos. Verifique os campos e tente novamente.';
    }
    if (err.status >= 500) {
      return 'Ocorreu um erro interno no servidor. Tente novamente em instantes.';
    }
    return 'Ocorreu um erro inesperado. Tente novamente.';
  }
}
