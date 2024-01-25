import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-edit-city-dialog',
  template: `
    <h2 mat-dialog-title>Edit City</h2>
    <div mat-dialog-content>
    <mat-form-field>
      <input matInput placeholder="City Name" [(ngModel)]="cityName">
    </mat-form-field>
    </div>
    <div mat-dialog-actions>
      <button mat-button (click)="onCancelClick()">Cancel</button>
      <button mat-button color="primary" (click)="onOkClick()">Ok</button>
    </div>
  `
})
export class EditCityDialogComponent {

  cityName: string;

  constructor(
    public dialogRef: MatDialogRef<EditCityDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { cityName: string }
  ) {
    // Initialize the input box with the original city name
    this.cityName = data.cityName;
  }

  onCancelClick(): void {
    // Close the dialog without saving changes
    this.dialogRef.close();
  }

  onOkClick(): void {
    // Close the dialog and pass the edited city name back to the calling component
    this.dialogRef.close(this.cityName);
  }
}
