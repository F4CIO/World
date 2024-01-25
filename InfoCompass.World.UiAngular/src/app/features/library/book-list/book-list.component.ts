import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { Title } from '@angular/platform-browser';
import { NotificationService } from 'src/app/core/services/notification.service';
import { environment } from 'src/environments/environment';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { City, Client, IUser, User } from 'src/app/shared/api-client.generated';
import { Subscription } from 'rxjs';
import { UntypedFormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import {JobStage, JobStageHelper} from 'src/app/core/models/job-stage';
import { interval} from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import * as moment from 'moment';
import { AddCityDialogComponent } from 'src/app/add-city-dialog/add-city-dialog.component';
import { EditCityDialogComponent } from 'src/app/edit-city-dialog-component/edit-city-dialog-component.component';


export interface BookListItem {
  position: number;
  momentOfCreation:string;
  momentOfLastUpdate:string;
  name: string;
  jobStage: string;
  jobStageDetails:string;
  jobProgressPercent:string;
  operation: string;
}

@Component({
  selector: 'app-book-list',
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.css']
})
export class BookListComponent implements OnInit {
  refreshIconName = 'refresh';
  addIconName = 'add';  
  saveIconName = 'save';
  private refreshIconNameForHourglass = "refresh"//"hourglass_empty"; //if you set different icon here user will see alternating icon every few seconds
  disableSubmit!: boolean;
  generalErrorMessage:string = '';  
  private timerSubscription: Subscription[] = [];
  private timerIntervalInMiliseconds = 5000;
  
  form!: UntypedFormGroup;

  displayedColumns: string[] = ['name','operation'];//'momentOfCreation', , 'jobStage', 'operation',
  dataSource:MatTableDataSource<BookListItem> = new MatTableDataSource([
    { position: 1, 
      momentOfCreation: "--",
      momentOfLastUpdate:"--",
      name: "----", 
      jobStage: "--",  
      jobStageDetails:"--",
      operation: '--'  
    } as BookListItem,
    { position: 2, 
      momentOfCreation: "--",
      momentOfLastUpdate:"--",
      name: "----", 
      jobStage: "--",  
      jobStageDetails:"--",
      operation: '--'  
    } as BookListItem,
    { position: 3, 
      momentOfCreation: "--",
      momentOfLastUpdate:"--",
      name: "----", 
      jobStage: "--",  
      jobStageDetails:"--",
      operation: '--'  
    } as BookListItem,
  ]);

  @ViewChild(MatSort, { static: true })
  sort: MatSort = new MatSort;

  constructor(
    private router: Router,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
    private logger: NGXLogger,
    private notificationService: NotificationService,
    private titleService: Title,
    private authService:AuthenticationService,
    private apiClient: Client
  ) { }

  ngOnInit() {
    this.titleService.setTitle(environment.INSTANCE_NAME+" - Library");
    this.logger.log('Library loaded');
    //this.notificationService.openSnackBar('Library loaded');
    this.dataSource.sort = this.sort;
    this.loadAll();

    //Uncomment below for auto-reload
    // this.tick();
    // this.timerSubscription.push(interval(this.timerIntervalInMiliseconds).subscribe((val) => { 
    //   this.tick();
    // }));
  }

  ngOnDestroy() {
    if(this.timerSubscription.length>0){
      this.timerSubscription.forEach(t=>t.unsubscribe());
      this.timerSubscription = [];
      console.log('timer stopped');
    }
  }

  tick(){
    if(this.loadAllSucceededLastTime){
      this.loadAll();      
      //increase interval and resubscribe
      if(this.timerSubscription.length>0){
        this.timerSubscription.forEach(t=>t.unsubscribe());
      }
      this.timerIntervalInMiliseconds = this.timerIntervalInMiliseconds+500;
      if(this.timerIntervalInMiliseconds<1*3600*1000){//resubscribe only if in first hour
        this.timerSubscription.push(interval(this.timerIntervalInMiliseconds).subscribe((val) => { 
          this.tick();
        }));
      }
    }else{
      if(this.timerSubscription.length>0){
        this.timerSubscription.forEach(t=>t.unsubscribe());
        this.timerSubscription = [];
        console.log('timer stopped');
      }
    }
  }

  loadAllSucceededLastTime = true;
  loadAll() {
    const userId = this.authService.getCurrentVisitor()?.id;    
    console.log(this.authService.getCurrentVisitor());
    // if(!userId){
    //   this.loadAllSucceededLastTime = false;
    // }else{
      console.log("Calling apiClient.getByUserId("+userId+")...");
      this.refreshIconName = this.refreshIconNameForHourglass;
      this.apiClient.getByUserId(userId)
        .subscribe({
          next: (serverResponseForUi) => {
            console.log(serverResponseForUi);
            if (!serverResponseForUi.isSuccess) {
              this.generalErrorMessage = serverResponseForUi.message || 'Error occured.';
              if (serverResponseForUi.errors && serverResponseForUi.errors.length>0) {
                console.log(serverResponseForUi.errors);
                for (var err of serverResponseForUi.errors) {
                  if(err.key){
                    const control = this.form.get(err.key);
                    if (control) {
                      control.setErrors({ controlSpecificError: err.friendlyMessage });
                      control.markAsTouched();
                    }
                  }
                }
                this.cdr.detectChanges();
                this.loadAllSucceededLastTime = false;  
              }
              this.notificationService.openSnackBar(this.generalErrorMessage);
            } else {
              var message = 'Loaded ' + serverResponseForUi.data?.length+' jobs.';
              console.log(message);
              this.logger.info(message);
              if(this.form){
                this.form.reset();
              }
              this.currentJobs = serverResponseForUi.data || [];
              this.jobsToDataSource(this.currentJobs);
              this.loadAllSucceededLastTime = true;          
            }
            this.disableSubmit = false;
            this.refreshIconName = 'refresh';
          },
          error: (error) => {
            this.loadAllSucceededLastTime = false;   
            this.generalErrorMessage = error;
            console.error(error);
            this.logger.error(error);
            this.notificationService.openSnackBar(error.error ? error.error : error.toString());
            this.disableSubmit = false;        
            this.refreshIconName = 'refresh';
          }
        });
    //}
  }

  currentJobs :City[] | undefined =[];
  longestStatusCharCount = 1;
  statusColumnWidth = '122px';

  jobsToDataSource(jobs:City[]){
    var r:BookListItem[] = [];

    var i:number = 1;
    jobs.forEach(job => {
      console.log(job);
      const momentOfCreation = job.momentOfCreation ? moment(job.momentOfCreation).format('YYYY.MM.DD HH:mm') : '?';
      const momentOfLastUpdate = 'Last update on '+(job.momentOfLastUpdate ? moment(job.momentOfLastUpdate).format('YYYY.MM.DD HH:mm') : '?');
      
      let jobStageDetails: string = "";
      
      var item:BookListItem  = { 
        position: i, 
        momentOfCreation: momentOfCreation,
        momentOfLastUpdate:momentOfLastUpdate,
        name: job?.name || '?', 
        jobStage:  '',  
        jobStageDetails:jobStageDetails,
        jobProgressPercent: '',
        operation: '-' };
      r.push(item);

      let statusCharCount = item.jobStage.length;
      if(statusCharCount>this.longestStatusCharCount){
        this.longestStatusCharCount = statusCharCount;
      }    

      i++;
    });

    //15:122=longestStatusCharCount:?
    this.statusColumnWidth = ((122*this.longestStatusCharCount)/15)+'px';

    this.dataSource = new MatTableDataSource(r);    
  }

  addNew(){
    const dialogRef = this.dialog.open(AddCityDialogComponent, {
      width: '300px',
      data: { cityName: '' } // Pass an empty string as the initial city name
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // User clicked 'Ok'
        const newCityName = result.trim(); // Trim any leading/trailing spaces

        if (newCityName) {
          // Add the new city to currentJobs
          this.currentJobs!.push({ name: newCityName } as City);
          // Update the data source and refresh the table
          this.jobsToDataSource(this.currentJobs || []);
        }
      }
      // User clicked 'Cancel' or closed the dialog
    });
  }

  

  saveAll(){
    const dummyUserData: IUser = {
      id: 1,
      eMail: 'dummy@example.com',
      passwordHash: 'hashedPassword',
      momentOfCreation: new Date(),
      momentOfLastUpdate: new Date(),
      momentOfLastLogin: new Date(),
      isRegistered: true,
      isAdministrator: false,
      isActive: true,
      firstName: 'John',
      lastName: 'Doe',
      momentOfLastVisit: new Date(),
      subscribed: false,
      paidPlanId: 2,
      note: 'This is a dummy user.',
    };
    
    this.currentJobs!.forEach(element => {
      element.id = 0;
      element.momentOfCreation = new Date();
      element.momentOfLastUpdate = new Date();
      element.user = new User(dummyUserData);;
      element.userId = 0;
      element.userEMailHint = 't@f4cio.com';
    });
    this.apiClient.saveAll(this.currentJobs)
        .subscribe({
          next: (serverResponseForUi) => {
            console.log(serverResponseForUi);
            if (!serverResponseForUi.isSuccess) {
              this.generalErrorMessage = serverResponseForUi.message || 'Error occured.';
              if (serverResponseForUi.errors && serverResponseForUi.errors.length>0) {
                console.log(serverResponseForUi.errors);
                for (var err of serverResponseForUi.errors) {
                  if(err.key){
                    const control = this.form.get(err.key);
                    if (control) {
                      control.setErrors({ controlSpecificError: err.friendlyMessage });
                      control.markAsTouched();
                    }
                  }
                }
                this.cdr.detectChanges();
                this.loadAllSucceededLastTime = false;  
              }
              this.notificationService.openSnackBar(this.generalErrorMessage);
            } else {
              var message = 'Loaded ' + serverResponseForUi.data?.length+' jobs.';
              console.log(message);
              this.logger.info(message);
              if(this.form){
                this.form.reset();
              }
              this.currentJobs = serverResponseForUi.data || [];
              this.jobsToDataSource(this.currentJobs);
              this.loadAllSucceededLastTime = true;          
            }
            this.disableSubmit = false;
            this.refreshIconName = 'refresh';

            this.showSnackBar('List saved to DB.');
          },
          error: (error) => {
            this.loadAllSucceededLastTime = false;   
            this.generalErrorMessage = error;
            console.error(error);
            this.logger.error(error);
            this.notificationService.openSnackBar(error.error ? error.error : error.toString());
            this.disableSubmit = false;        
            this.refreshIconName = 'refresh';
          }
        });
  }

  editItem(element: BookListItem) {
    // Implement the edit action here
    console.log('Edit item:', element);

    const dialogRef = this.dialog.open(EditCityDialogComponent, {
      width: '300px',
      data: { cityName: element.name } // Pass the current city name to the dialog
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // User clicked 'Ok'
        const updatedCityName = result.trim(); // Get the updated city name

        if (updatedCityName) {
          // Find the index of the element to be updated
          const index = this.currentJobs!.findIndex(city => city.name === element.name);

          if (index !== -1) {
            // Update the city name in currentJobs
            this.currentJobs![index].name = updatedCityName;
            // Update the data source and refresh the table
            this.jobsToDataSource(this.currentJobs || []);
            this.showSnackBar('Success! Click save to preserve list to DB.');
          } else {
            this.showSnackBar('City not found.');
          }
        }
      }
      // User clicked 'Cancel' or closed the dialog
    });
  }

  deleteItem(element: BookListItem) {
    // Implement the delete action here
    console.log('Delete item:', element);
    let indexToDelete = -1;

  for (let i = 0; i < (this.currentJobs || []).length; i++) {
    if (this.currentJobs![i].name === element.name) {
      indexToDelete = i;
      break; // Stop iterating once the match is found
    }
  }

  if (indexToDelete >= 0) {
    this.currentJobs!.splice(indexToDelete, 1);
    this.jobsToDataSource(this.currentJobs || []); // Update the data source and refresh the table
    this.showSnackBar('Success! Click save to preserve list to DB.');
  } else {
    this.showSnackBar('Item not found.');
  }
  }

  showSnackBar(message:string) {
    this.notificationService.openSnackBar(message);
  }
}
