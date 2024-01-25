import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-add-city-dialog',
  template: `
    <h2 mat-dialog-title>Add New City</h2>
    <div mat-dialog-content>
      <mat-form-field>
        <input matInput [(ngModel)]="data.cityName" placeholder="City Name" />
      </mat-form-field>
    </div>
    <div mat-dialog-actions>
      <button mat-button (click)="onCancelClick()">Cancel</button>
      <button mat-button (click)="onOkClick()">Ok</button>
    </div>
  `,
})
export class AddCityDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<AddCityDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { cityName: string }
  ) {}

  onCancelClick(): void {
    this.dialogRef.close();
  }

  onOkClick(): void {
    this.dialogRef.close(this.data.cityName);
  }
}
