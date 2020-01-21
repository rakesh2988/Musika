import { Component, OnInit, ViewChild, ElementRef, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { FormGroup, FormBuilder, Validators, FormArray } from '@angular/forms';
import { User, Ticket, TicketData } from '../../../models/usermodel';
import { debug } from 'util';
import { IMyDpOptions, IMyDate, IMyDateModel, IMyOptions } from 'mydatepicker';
import { MyDatePickerModule } from 'mydatepicker';
import { AmazingTimePickerService } from 'amazing-time-picker';
import { DomSanitizer } from '@angular/platform-browser';
import { HeaderService } from '../../../header/nav-menu.service';
import { NgxSpinnerService } from 'ngx-spinner';


@Component({
  selector: 'eventcreation-component',
  templateUrl: './eventcreation.component.html',
  styleUrls: ['./eventcreation.component.css']
})

export class EventCreationComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  showHide: boolean = false;
  registerForm: FormGroup;
  title: string = "Create";
  EventID: number;
  errorMessage: any;
  currentUser: User;
  users: User[] = [];
  CountryId: number = 55;
  Currency: string = "DOP";
  RefundPolicy: string;
  ServiceFee: string = "15";
  Tax: string="18";
  ticket: Ticket = { EventId: 0, Currency: "", CountryId: 0, RefundPolicy: "", ServiceFee: "", Tax:"", lstTicketData: [{ EventId: 0, Quantity: 0, Price: "", TicketCategory: "", PackageStartDate: "", PackageEndDate: "" }] };
  imagepath: any;
  public selectedTime: string;
  countrylist: any;
  Userlist: any;
  Venuelist: any;
  Artistlist: any;
  policy = null;
  msgcss: string = '';
  IsValidImagemsg: string = '';
  IsMessage: boolean = false;
  responseMessage: string = '';
  //IsPopup: boolean = false;
  IsFlag: boolean = true;
  ImageError: boolean = false;
  Isprice: boolean = false;
  IsValidImage: boolean = false;
  dropdownList = [];
  selectedItems = [];
  dropdownSettings = {};
  today = new Date();
  public ClockStartTime: string = '';
  public ClockEndTime: string = '';
  arrayPackageDates = [{ PackageStart: "", PackageEnd: "" }];
  //public myDatePickerOptions = {
  //  dateFormat: 'yyyy-mm-dd',
  //  disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate() }

  //};
  public startDate: IMyOptions = {
    // start date options here...
    openSelectorOnInputClick: true,
    inline: false,
    showClearDateBtn: false,
    editableDateField: false,
    dateFormat: 'yyyy-mm-dd',
    disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate() - 1 }
  };
  public endDate: IMyOptions = {
    // other end date options here...
    openSelectorOnInputClick: true,
    inline: false,
    showClearDateBtn: false,
    editableDateField: false,
    dateFormat: 'yyyy-mm-dd',
    disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate() - 1 }
  }
  public packagestartdate: IMyOptions = {
    // start date options here...
    openSelectorOnInputClick: true,
    inline: false,
    showClearDateBtn: false,
    editableDateField: false,
    dateFormat: 'yyyy-mm-dd',
    disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate() - 1 }
  };
  public packageenddate: IMyOptions = {
    // other end date options here...
    openSelectorOnInputClick: true,
    inline: false,
    showClearDateBtn: false,
    editableDateField: false,
    dateFormat: 'yyyy-mm-dd',
    disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate() - 1 }
  }
  public newPackageEndDate: IMyOptions = {
    // other end date options here...
    openSelectorOnInputClick: true,
    inline: false,
    showClearDateBtn: false,
    editableDateField: false,
    dateFormat: 'yyyy-mm-dd',
    disableUntil: { year: this.today.getFullYear(), month: this.today.getMonth() + 1, day: this.today.getDate() - 1 }
  }
  public ticketArray: Array<TicketData> = [];
  public newTicketRow: any = {};


  public arrDate: IMyDate = { year: 0, month: 0, day: 0 };
  public arrStartDate: IMyDate = { year: 0, month: 0, day: 0 };

  public packagearrDate: IMyDate = {
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    day: new Date().getDate()
  };
  public newpackagearrDate: IMyDate = {
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    day: new Date().getDate()
  };
  public packagedepDate: IMyDate = {
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    day: new Date().getDate()
  };
  public newpackagedepDate: IMyDate = {
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    day: new Date().getDate()
  };
  public arrDT: string = '';
  public packageStDate: string = '';
  public packageEdDate: string = '';

  public depDate: IMyDate = { year: 0, month: 0, day: 0 };
  public depEndDate: IMyDate = { year: 0, month: 0, day: 0 };
  public depDT: string = '';
  selectedFile: File;
  @ViewChild('fileInput') fileInput: ElementRef;
  @ViewChild('Artist') private ArtistRef: ElementRef;
  @ViewChild('Venue') private VenueRef: ElementRef;
  @ViewChild('ticket') private TicketRef: ElementRef;
  @ViewChild('Location') private LocationRef: ElementRef;
  @ViewChild('Title') private TitleRef: ElementRef;
  @ViewChild('price') private PriceRef: ElementRef;

  disabled: boolean = false;
  public _items: string[] = ["Appearance or Signing", "Attraction", "Concert or Performance", "Dinner or Gala"
    , "Festival or Fair", "Game or Competition", "Meeting or Networking Event", "Other", "Party or Social Gathering"
    , "Tour", "Tradeshow, Consumer Show, or Expo"];

  constructor(private route: ActivatedRoute, private atp: AmazingTimePickerService, private router: Router,
    private RvCellService: RVCellService, public nav: HeaderService, private _fb: FormBuilder, private sanitizer: DomSanitizer
    , private spinner: NgxSpinnerService
  ) {
    this.newTicketRow = { TicketType: "select", TicketCategory: "", Quantity: "", Price: "", PackageStartDate: "", PackageEndDate: "", EventId: "0" }
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);
    }

    else {

      if (this.route.snapshot.params["id"]) {
        this.EventID = this.route.snapshot.params["id"];
      }
      this.bindCountrydata();
      this.bindStaffList();
      this.bindVenueList();
      this.bindArtistList();
      this.registerForm = this._fb.group({
        EventID: 0,
        ArtistId: [''],
        ArtistName: [''],
        EventTitle: ['', [Validators.required]],
        EventLocation: [''],
        VenueName: ['', [Validators.required]],
        Address1: [''],
        Address2: [''],
        City: [''],
        State: [''],
        ZipCode: [''],
        StartDate: [new Date(), [Validators.required]],
        StartTime: ['', [Validators.required]],
        EndDate: [new Date(), [Validators.required]],
        EndTime: ['', [Validators.required]],
        EventImage: ['', [Validators.required]],
        EventDescription: ['', [Validators.required]],
        OrganizerName: [''],
        OrganizerDescription: [''],
        TicketType: [''],
        ListingPrivacy: [''],
        EventType: [''],
        EventTopic: [''],
        ShowTicketNumbers: [''],
        CreatedBy: [this.currentUser.UserID],
        CreatedOn: [new Date()],
        //StaffId: [''],
        lstStaff: [''],
        Isdeleted: null,
        IsApproved: null,
        TicketUrl: [''],
        NumberOfTickets: [''],
        Ticket: [],
        Latitude: [''],
        Longitude: [''],
        MessageResponse: [''],
        lstTicketingEventsModel: [''],
        TotalTickets: [''],
        TicketsSold: ['']
      });

      let d: Date = new Date();
      this.arrDate = {
        year: d.getFullYear(),
        month: d.getMonth() + 1,
        day: d.getDate()
      };
      this.arrDT = this.arrDate.year + "-" + this.arrDate.month + "-" + this.arrDate.day;//JSON.stringify(this.arrDate);


      this.depDate = {
        year: d.getFullYear(),
        month: d.getMonth() + 1,
        day: d.getDate()
      };
      this.depDT = this.depDate.year + "-" + this.depDate.month + "-" + this.depDate.day; //JSON.stringify(this.depDate);


      this.arrStartDate = {
        year: d.getFullYear(),
        month: d.getMonth() + 1,
        day: d.getDate()
      };
      this.packageStDate = this.arrStartDate.year + "-" + this.arrStartDate.month + "-" + this.arrStartDate.day;//JSON.stringify(this.arrDate);
      this.depEndDate = {
        year: d.getFullYear(),
        month: d.getMonth() + 1,
        day: d.getDate()
      };
      this.packageEdDate = this.depEndDate.year + "-" + this.depEndDate.month + "-" + this.depEndDate.day; //JSON.stringify(this.depDate);


    }
  }


  onFileChange(event) {
    this.IsValidImage = false;
    let fileList: FileList = event.target.files;
    if (fileList.length > 0) {
      //(event.target.files[0].size / 1024).toFixed(2);
      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
      var imagetype = event.target.files[0].type.split('/').pop().toLowerCase();
      if (imagetype != "jpeg" && imagetype != "jpg" && imagetype != "png" && imagetype != "bmp" && imagetype != "gif") {
        event.target.value = "";
        event = null;
        fileList = null;
        this.imagepath = "";
        this.registerForm.controls.EventImage.setValue('');
        //this.registerForm.controls.EventImage.status = "INVALID";
        this.IsValidImage = true;
        this.IsValidImagemsg = "Please upload file having extensions .jpeg/.jpg/.png/.gif only.";
        return;
      }
      let v = parseFloat((event.target.files[0].size / 1024).toFixed(2));
      if (v > 10000) {
        event.target.value = "";
        event = null;
        fileList = null;
        this.imagepath = "";
        this.registerForm.controls.EventImage.setValue('');
        //this.registerForm.controls.EventImage.status = "INVALID";
        this.IsValidImage = true;
        this.IsValidImagemsg = "Image Size Should less than 10MB";
       
        return;
      }
      let file: File = fileList[0];
      var img = document.querySelector("#preview img");
      var reader1 = new FileReader();
      reader1.onload = (event: any) => {
        // The file's text will be printed here
        this.imagepath = event.target.result;

        this.registerForm.controls['EventImage'].setValue(event.target.result)
        this.ImageError = false;
      };
      reader1.readAsDataURL(event.target.files[0]);
    }
  }
  openStartTime(ev: any) {
    const amazingTimePicker = this.atp.open();
    amazingTimePicker.afterClose().subscribe(time => {
      this.ClockStartTime = time;
      this.registerForm.controls['StartTime'].setValue(time);

    });
  }
  openEndTime(ev: any) {
    const amazingTimePicker = this.atp.open();
    amazingTimePicker.afterClose().subscribe(time => {
      this.ClockEndTime = time;
      this.registerForm.controls['EndTime'].setValue(time);

    });
  }
  onArrDateChanged(event: IMyDateModel) {
    // Update value of selDate variable
    this.arrDate = event.date;
    this.arrDT = event.formatted;
    let d: Date = event.jsdate;

    // set previous date
    d.setDate(d.getDate() - 1);
    let copy: IMyOptions = this.getCopyOfEndDateOptions();
    copy.disableUntil = { year: d.getFullYear(), month: d.getMonth() + 1, day: d.getDate() };
    this.endDate = copy;
  }

  onDepDateChanged(event: IMyDateModel) {
    // Update value of selDate variable
    this.depDate = event.date;
    this.depDT = event.formatted;

    //if (this.arrDT > this.depDT) {
    //  this.IsPopup = true;
    //  //alert("The End date should greater than Start Date!");
    //}
  }
  onArrStartDateChanged(event: IMyDateModel) { //package startdate
    let d: Date = event.jsdate;
    this.newPackageEndDate = null;
    this.arrStartDate = event.date;
    this.packageStDate = event.formatted;


    // set previous date
    d.setDate(d.getDate() - 1);
    let copy: IMyOptions = this.getCopyOfEndDateOptions();
    copy.disableUntil = { year: d.getFullYear(), month: d.getMonth() + 1, day: d.getDate() };
    this.newPackageEndDate = copy;
  }
  onArrEndDateChanged(event: IMyDateModel) {//package ENDDATE 
    // Update value of selDate variable
    this.depEndDate = event.date;
    this.packageEdDate = event.formatted;
  }
  getCopyOfEndDateOptions(): IMyOptions {
    return JSON.parse(JSON.stringify(this.endDate));
  }
  //closepopup() {
  //  this.IsPopup = false;
  //}
  clearDate() {
    this.arrDate = { year: 0, month: 0, day: 0 };
    this.depDate = { year: 0, month: 0, day: 0 };
  }
  ngOnInit() {
    this.dropdownSettings = {
      singleSelection: false,
      idField: 'UserId',
      textField: 'UserName',
      selectAllText: 'Select All',
      unSelectAllText: 'UnSelect All',
      allowSearchFilter: true
    };


    // reset login status


    if (this.EventID > 0) {
      this.title = "Edit";
      this.spinner.show();
      
      this.RvCellService.getEventById(this.EventID.toString())
        .subscribe((resp) => {
          
          var StartDate = resp.StartDate.split("T");
          resp.StartDate = StartDate[0];
          this.arrDate = StartDate[0];

          var EndDate = resp.EndDate.split("T");
          resp.EndDate = EndDate[0];
          var d = new Date(resp.StartDate);
          d.toUTCString();
          //let d: Date = new Date(resp.StartDate).toUTCString();
          // set previous date

          d.setDate(d.getUTCDate() - 1);
          let copy: IMyOptions = this.getCopyOfEndDateOptions();
          copy.disableUntil = { year: d.getUTCFullYear(), month: d.getUTCMonth() + 1, day: d.getUTCDate() };
          this.endDate = copy;
          //////
          this.depDate = EndDate[0];
          if (resp.Ticket != null) {
            this.CountryId = resp.Ticket.CountryId;
            this.Currency = resp.Ticket.Currency;
            this.RefundPolicy = resp.Ticket.RefundPolicy;
            this.ServiceFee = resp.Ticket.ServiceFee;
            this.Tax = resp.Ticket.Tax;
            if (resp.Ticket.lstTicketData.length > 0) {
              if (resp.Ticket.lstTicketData[0].EventId == 0) {
                resp.Ticket.lstTicketData.length = 0
              }
              else {
                resp.Ticket.lstTicketData.forEach(function (a) {
                  if (a.PackageStartDate != "") {
                    var PackageStartDate = new Date(a.PackageStartDate);
                    PackageStartDate.toUTCString();
                    a.PackageStartDate = { 'date': { 'year': PackageStartDate.getUTCFullYear(), 'month': PackageStartDate.getUTCMonth() + 1, 'day': PackageStartDate.getUTCDate() }, 'jsdate': a.PackageStartDate + 'T18:30:00.000Z', 'formatted': a.PackageStartDate, 'epoc': PackageStartDate.getTime() };
                  }
                  if (a.PackageEndDate != "") {
                    var PackageEndDate = new Date(a.PackageEndDate);
                    PackageEndDate.toUTCString();
                    a.PackageEndDate = { 'date': { 'year': PackageEndDate.getUTCFullYear(), 'month': PackageEndDate.getUTCMonth() + 1, 'day': PackageEndDate.getUTCDate() }, 'jsdate': a.PackageEndDate + 'T18:30:00.000Z', 'formatted': a.PackageEndDate, 'epoc': PackageEndDate.getTime() };
                  }
                });
                this.ticketArray = resp.Ticket.lstTicketData;
              }
            }
          }
          resp.EndTime = this.convertAMPMTimeFormat(resp.EndTime);//.replace("AM", "").replace("PM", "");
          resp.StartTime = this.convertAMPMTimeFormat(resp.StartTime);//.replace("AM", "").replace("PM", "");

          if ((resp.EventImage == null) || (resp.EventImage == "")) {

          }
          else {
            this.imagepath = "data:image/jpeg;base64," + resp.EventImage.split(',')[0];
            resp.EventImage = "data:image/jpeg;base64," + resp.EventImage.split(',')[0];
          }
          this.selectedItems = resp.lstStaff;
          this.spinner.hide();
          this.registerForm.setValue(resp);
        }, error => this.errorMessage = error);
    }
    // get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }
  convertTimeFormatAMPM(time) {
    if (time != "") {
      let hour = (time.split(':'))[0]
      let min = (time.split(':'))[1]
      let part = hour >= 12 ? 'PM' : 'AM';
      min = (min + '').length == 1 ? `0${min}` : min;
      hour = hour > 12 ? hour - 12 : hour;
      hour = (hour + '').length == 1 ? `0${hour}` : hour;
      return `${hour}:${min} ${part}`
    }
    return "";
  }
  convertAMPMTimeFormat(time) {
    if (time.includes("PM") || time.includes("AM")) {
      var time = time;
      var hours = Number(time.match(/^(\d+)/)[1]);
      var minutes = Number(time.match(/:(\d+)/)[1]);
      var AMPM = time.match(/\s(.*)$/)[1];
      if (AMPM == "PM" && hours < 12) hours = hours + 12;
      if (AMPM == "AM" && hours == 12) hours = hours - 12;
      var sHours = hours.toString();
      var sMinutes = minutes.toString();
      if (hours < 10) sHours = "0" + sHours;
      if (minutes < 10) sMinutes = "0" + sMinutes;
      return sHours + ":" + sMinutes;
    }
    return time;
  }

  OnTicketCategory(value) {

    this.Isprice = false;
    if (value == "Paid") {
      this.IsFlag = true;
      this.Isprice = true;
    }
    else if (value == "select") {
      this.IsFlag = true;
    }
    else {
      this.IsFlag = false;
    }

  }

  addFieldValue() {
    this.Isprice = false;
    if (this.newTicketRow.TicketType == "Paid" && (this.newTicketRow.Price == "" || this.newTicketRow.Price == undefined)) {
      this.Isprice = true;
      return;
    }
    if ((this.newTicketRow.TicketCategory) && (this.newTicketRow.Quantity)) {
      this.newTicketRow["EventId"] = "0";

      var tempTicketRowSecond = this.newTicketRow;
      this.ticketArray.push(tempTicketRowSecond);

      this.newTicketRow = { TicketType: "select", TicketCategory: "", Quantity: "", Price: "", PackageStartDate: "", PackageEndDate: "", EventId: "0" };
    }

  }
  OnTicketCategoryEditChange(value, index) {
    this.Isprice = false;
    if (value != "Paid") {
      this.ticketArray[index].Price = "";
    }
  }
  deleteFieldValue(index) {
    this.ticketArray.splice(index, 1);
  }
  bindCountrydata() {
    this.RvCellService.getCountryList()
      .subscribe((data) => {
        this.countrylist = data.Response.Message;
      }, error => this.errorMessage = error)
  }
  bindStaffList() {
    this.RvCellService.getStaffList(this.currentUser.UserID.toString())
      .subscribe((data) => {
        this.Userlist = data.Response.Message;
      }, error => this.errorMessage = error)
  }
  bindVenueList() {
    this.RvCellService.getVenueList()
      .subscribe((data) => {

        this.Venuelist = data.Response.Message;
      }, error => this.errorMessage = error)
  }

  bindArtistList() {
    this.RvCellService.getArtistList()
      .subscribe((data) => {
        this.Artistlist = data.Response.Message;
      }, error => this.errorMessage = error)
  }
  onSelect(countryid) {
    this.RvCellService.getVenueDetailsbyName(countryid)
      .subscribe((data) => {
        if (data.Response.Message) {

          this.registerForm.controls['Address1'].setValue(data.Response.Message[0].Address ? data.Response.Message[0].Address : "");
          this.registerForm.controls['Address2'].setValue(data.Response.Message[0].Extended_Address ? data.Response.Message[0].Extended_Address : "");
          this.registerForm.controls['City'].setValue(data.Response.Message[0].VenueCity ? data.Response.Message[0].VenueCity : "");
          this.registerForm.controls['State'].setValue(data.Response.Message[0].VenueState ? data.Response.Message[0].VenueState : "");
          if (data.Response.Message[0].Postal_Code == "null") {
            this.registerForm.controls['ZipCode'].setValue("");
          }
          else {
            this.registerForm.controls['ZipCode'].setValue(data.Response.Message[0].Postal_Code ? data.Response.Message[0].Postal_Code : "");
          }
        }

      }, error => this.errorMessage = error)
  }

  public doSelect = (value: any) => {
    this.RvCellService.getVenueDetailsbyName(value)
      .subscribe((data) => {
        if (data.Response.Message) {

          this.registerForm.controls['Address1'].setValue(data.Response.Message[0].Address ? data.Response.Message[0].Address : "");
          this.registerForm.controls['Address2'].setValue(data.Response.Message[0].Extended_Address ? data.Response.Message[0].Extended_Address : "");
          this.registerForm.controls['City'].setValue(data.Response.Message[0].VenueCity ? data.Response.Message[0].VenueCity : "");
          this.registerForm.controls['State'].setValue(data.Response.Message[0].VenueState ? data.Response.Message[0].VenueState : "");
          if (data.Response.Message[0].Postal_Code == "null") {
            this.registerForm.controls['ZipCode'].setValue("");
          }
          else {
            this.registerForm.controls['ZipCode'].setValue(data.Response.Message[0].Postal_Code ? data.Response.Message[0].Postal_Code : "");
          }
        }

      }, error => this.errorMessage = error)
  }
  save() {
    debugger;
    if (!this.registerForm.valid) {
      if (this.registerForm.controls.EventTitle.status == "INVALID") {
        this.TitleRef.nativeElement.focus();
        return;
      }
      if (this.Isprice) {
        this.PriceRef.nativeElement.focus();
        return;
      }

      if (this.registerForm.controls.VenueName.status == "INVALID") {
        this.VenueRef.nativeElement.focus();
        return;
      }

      if (this.registerForm.controls.EventImage.status == "INVALID") {
        this.fileInput.nativeElement.focus();
        this.ImageError = true;
        return;
      }
      //if (this.registerForm.controls.ArtistId.status == "INVALID") {
      //  this.ArtistRef.nativeElement.focus();
      //  return;
      //}
      return;
    }
    this.spinner.show();

    this.registerForm.controls['CreatedBy'].setValue(this.currentUser.UserID);
    if (this.title == "Create") {
      this.ticket.Currency = this.Currency;
      this.ticket.CountryId = this.CountryId
      this.ticket.RefundPolicy = this.RefundPolicy;
      this.ticket.EventId = this.registerForm.value.EventID;
      this.ticket.Tax = this.Tax.replace(/[^0-9 ]/g, "") + "%";
      this.ticket.ServiceFee = this.ServiceFee.replace(/[^0-9 ]/g, "") + "%";
      this.ticket.lstTicketData = this.ticketArray;
      this.ticketArray.forEach(function (a) {
        a.PackageStartDate = JSON.stringify(a.PackageStartDate);
        a.PackageEndDate = JSON.stringify(a.PackageEndDate);
      });
      this.registerForm.controls['Ticket'].setValue(this.ticket);
      this.registerForm.controls['StartDate'].setValue(this.arrDT);
      this.registerForm.controls['EndDate'].setValue(this.depDT);
      var obj = this.registerForm.value;
      obj.StartTime = this.convertTimeFormatAMPM(this.registerForm.value.StartTime);
      obj.EndTime = this.convertTimeFormatAMPM(this.registerForm.value.EndTime);
      obj.lstStaff = this.selectedItems;
      this.RvCellService.eventcreation(obj)
        .subscribe((data) => {
          if (data.ReturnMessage == "Success") {
            this.spinner.hide();
            this.msgcss = "text-success";
            this.IsMessage = true;
            this.responseMessage = "Event added Successfully.";
            this.router.navigate(['/eventlisting']);
          }
          else {
            this.spinner.hide();
            this.msgcss = "text-danger";
            this.IsMessage = true;
            this.responseMessage = data.ReturnMessage;

          }
        }, error => this.errorMessage = error)

    }
    else if (this.title == "Edit") {

      this.ticket.Currency = this.Currency;
      this.ticket.CountryId = this.CountryId
      this.ticket.RefundPolicy = this.RefundPolicy;
      this.ticket.Tax = this.Tax.replace(/[^0-9 ]/g, "") + "%";
      this.ticket.ServiceFee = this.ServiceFee.replace(/[^0-9 ]/g, "") + "%";
      this.ticket.EventId = this.registerForm.value.EventID;
      this.ticketArray.forEach(function (a) {
        a.PackageStartDate = JSON.stringify(a.PackageStartDate)
        a.PackageEndDate = JSON.stringify(a.PackageEndDate)
      });
      this.ticket.lstTicketData = this.ticketArray;
      this.registerForm.controls['Ticket'].setValue(this.ticket);
      this.registerForm.controls['StartDate'].setValue(this.registerForm.value.StartDate.formatted ? this.registerForm.value.StartDate.formatted : this.registerForm.value.StartDate);
      this.registerForm.controls['EndDate'].setValue(this.registerForm.value.EndDate.formatted ? this.registerForm.value.EndDate.formatted : this.registerForm.value.EndDate);
      var obj = this.registerForm.value;
      obj.StartTime = this.convertTimeFormatAMPM(this.registerForm.value.StartTime);
      obj.EndTime = this.convertTimeFormatAMPM(this.registerForm.value.EndTime);
      obj.lstStaff = this.selectedItems;
      var strtdate = this.registerForm.value.StartDate.formatted ? this.registerForm.value.StartDate.formatted : this.registerForm.value.StartDate;
      var enddate = this.registerForm.value.EndDate.formatted ? this.registerForm.value.EndDate.formatted : this.registerForm.value.EndDate;
      this.RvCellService.eventUpdation(obj)
        .subscribe((data) => {
          this.spinner.hide();
          this.router.navigate(['/eventlisting']);
        }, error => this.errorMessage = error)
    }
  }

  get EventTitle() { return this.registerForm.get('EventTitle'); }
  // get EventLocation() { return this.registerForm.get('EventLocation'); }
  get VenueName() { return this.registerForm.get('VenueName'); }
  //get TicketUrl() { return this.registerForm.get('TicketUrl'); }
  get ArtistId() { return this.registerForm.get('ArtistId'); }
  get StartDate() { return this.registerForm.get('StartDate'); }
  get StartTime() { return this.registerForm.get('StartTime'); }
  get EndDate() { return this.registerForm.get('EndDate'); }
  get EndTime() { return this.registerForm.get('EndTime'); }
  get EventDescription() { return this.registerForm.get('EventDescription'); }
}
