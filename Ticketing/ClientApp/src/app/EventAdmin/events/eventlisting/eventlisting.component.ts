import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { User } from '../../../models/usermodel';
import { HeaderService } from '../../../header/nav-menu.service';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'eventlisting-component',
  templateUrl: './eventlisting.component.html',
  styleUrls: ['./eventlisting.component.css']
})

export class EventListingComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  userID: number = 0;
  currentUser: User;
  users: User[] = [];
  errorMessage: any;
  public eventList: User[];
  constructor(private route: ActivatedRoute, private router: Router, public nav: HeaderService, private RvCellService: RVCellService
    , private spinner: NgxSpinnerService) {
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/login']);
    }

    else {
      this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    }
  }

  ngOnInit() {
    this.GetAllEvents();
  }

  GetAllEvents() {
    this.spinner.show();
    this.RvCellService.eventlisting(this.currentUser.UserID.toString())
      .subscribe(data => {
        this.eventList = data;
        this.spinner.hide();
      },
        error => {
          //this.loading = false;
        });
  }

  delete(eventId) {
    var ans = confirm("Do you want to delete this Event");
    if (ans) {
      this.spinner.show();
      this.RvCellService.deleteEvent(eventId)
        .subscribe((data) => {
          this.GetAllEvents();
        }, error => this.errorMessage = error);
    }
  }
  ExportCSV(eventId) {
      this.spinner.show();
      this.RvCellService.ExportEventAdmin(eventId)
        .subscribe((data) => {
          window.open(data, "_blank");
          this.spinner.hide();
        }, error => { this.spinner.hide(); this.errorMessage = error });
      //this.RvCellService.ExportEvent(eventId)
      //  .subscribe((data) => {
      //    
          

      //    var options = {
      //      fieldSeparator: ',',
      //      quoteStrings: '"',
      //      decimalseparator: '.',
      //      showLabels: true,
      //      showTitle: true,
      //      useBom: true,
      //      noDownload: false
      //    };
      //    var result = [];
      //    //result.push(data.Response.CSV.Venue);
      //    //data.Response.CSV.Venue = "";

      //    for (var i in data.Response.CSV.lstTicket[0].lstTicketData) {
      //      result.push(data.Response.CSV.lstTicket[0].lstTicketData[i]);
      //    }
      //    data.Response.CSV.lstTicket[0].lstTicketData = "";
      //    //for (var i in data.Response.CSV.lstTicket) {
      //    //  result.push(data.Response.CSV.lstTicket[i]);
      //    //}
      //    for (var i in data.Response.CSV.UsersGoing) {
      //      result.push(data.Response.CSV.UsersGoing[i]);
      //    }
      //    for (var i in data.Response.CSV.ArtistRelated) {
      //      result.push(data.Response.CSV.ArtistRelated[i]);
      //    }
      //    data.Response.CSV.ArtistRelated = "";
      //    data.Response.CSV.UsersGoing = "";
      //    data.Response.CSV.lstTicket = "";

      //    result.push(data.Response.CSV);
      //    var data, filename, link;
      //    var csv = this.convertArrayOfObjectsToCSV({
      //      data: result
      //    });
      //    if (csv == null) return;

      //    filename = data.Response.CSV.Event_Name + ".csv";

      //    if (!csv.match(/^data:text\/csv/i)) {
      //      csv = 'data:text/csv;charset=utf-8,' + csv;
      //    }
      //    data = encodeURI(csv);

      //    link = document.createElement('a');
      //    link.setAttribute('href', data);
      //    link.setAttribute('download', filename);
      //    link.click();

      //    var data1 = new Angular5Csv(result, data.Response.CSV.Event_Name);

      //  }, error => this.errorMessage = error);
    
  }

  //convertArrayOfObjectsToCSV(args) {
  //  var result, ctr, keys, columnDelimiter, lineDelimiter, data,duplicateheader;

  //  data = args.data || null;
  //  if (data == null || !data.length) {
  //    return null;
  //  }

  //  columnDelimiter = args.columnDelimiter || ',';
  //  lineDelimiter = args.lineDelimiter || '\n\n';
  //  result = '';
  //  data.forEach(function (item) {
  //    ctr = 0;
  //    keys = Object.keys(item);
  //    //duplicateheader += keys;
  //    //if (duplicateheader.contains(keys)){
  //    //  keys = '';
  //    //}
  //    result += keys.join(columnDelimiter);
  //    result += lineDelimiter;
  //    keys.forEach(function (key) {
  //      if (ctr > 0) result += columnDelimiter;

  //      result += item[key];
  //      ctr++;
  //    });
  //    result += lineDelimiter;
  //  });

  //  return result;
  //}

}

