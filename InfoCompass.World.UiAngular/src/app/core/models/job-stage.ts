export enum JobStage
{
	Unknown = -1,
	NotStartedYet = 0,
	Initializing = 10,
	Generating = 20,
	Exporting = 30,
	SendingEMail = 40,
	FinishedWithSuccess = 50,
	FinishedWithError = 60
}

export class JobStageHelper
{
	 static toString(jobStage:JobStage):string{
		let r:string;
		switch(jobStage)
		{
			case JobStage.Unknown: r = 'Unknown';break;			
			case JobStage.NotStartedYet: r = 'Not started yet';break;						
			case JobStage.Initializing: r = 'Initializing...';break;						
			case JobStage.Generating: r = 'Generating...';break;						
			case JobStage.Exporting: r = 'Exporting...';break;						
			case JobStage.SendingEMail: r = 'Sending eMail...';break;						
			case JobStage.FinishedWithSuccess: r = 'Finished & Sent';break;						
			case JobStage.FinishedWithError: r = 'Failed';break;		
			default: r = JobStage[jobStage];
		}
		return r;
	 }
}