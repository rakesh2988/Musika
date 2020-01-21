import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../../../service/service';
import { User } from '../../../models/usermodel';
import { FormBuilder, FormGroup } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts/ng2-charts';
import { HeaderService } from '../../../header/nav-menu.service';
import { NgxSpinnerService } from 'ngx-spinner';
@Component({
  selector: 'ticketsAnalytics-component',
  templateUrl: './ticketsAnalytics.component.html',
  styleUrls: ['./ticketsAnalytics.component.css']
})

export class TicketsAnalyticsComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  userID: number = 0;
  currentUser: User;
  EventID: number;
  users: User[] = [];
  errorMessage: any;
  eventAnalytics: any;
  eventUsers: any;
  @ViewChild(BaseChartDirective) private _chart;
  public isDataAvailableGender: boolean = false;
  public isDataAvailableAge: boolean = false;
  public isDataAvailableTicket: boolean = false;
  //public chartType: string = 'pie';
  @Input() chartLabels: Array<string> = [];
  @Input() chartData: Array<number> = [];

  @Input() chartOptions: any = {}

  @Input() chartLabelsGender: Array<string> = [];
  @Input() chartDataGender: Array<number> = [];

  @Input() chartOptionsGender: any = {}
  @Input() barLabels: Array<string> = [];
  @Input() barData: any = {};

  @Input() barOptions: any = {};
  @Input() lineChartData: any = {};
  @Input() lineChartLabels: any = {};
  @Input() lineChartOptions: any = {};

  public isDataAvailable: boolean = false;
  public lineChartLegend: boolean = true;
  public lineChartType: string = 'line';

  ticketTotal: Array<string> = [];
  ticketsoldTotal: Array<string> = [];
  ticketattendedTotal: Array<string> = [];
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private _fb: FormBuilder,
    public nav: HeaderService,
    private RvCellService: RVCellService, private spinner: NgxSpinnerService) {


    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);
    }
    
    else {
      if (this.route.snapshot.params["id"]) {
        this.EventID = this.route.snapshot.params["id"];
      }
    }
  }

  ngOnInit() {

    this.GetEventAnalytics(this.EventID);
    //this.GetEventUsers(this.EventID);
    this.GetEventBarAnalytics(this.EventID);
    this.GetEventBarGenderAnalytics(this.EventID);
    this.GetEventLineAnalytics(this.EventID);
  }

  forceChartRefresh() {
    setTimeout(() => {
      this._chart.refresh();
    }, 10);
  }
  //total tickets analytics
  GetEventAnalytics(eventID) {
    this.spinner.show();
    this.RvCellService.getEventAnalytics(eventID)
      .subscribe(data => {
        this.eventAnalytics = data
        this.chartLabels = ["Total Tickets", "Sold Tickets", "Attended"];
        this.chartData = [data.TotalTickets, data.TotalSales, data.Attendees];
        this.chartOptions = {
          pieceLabel: {
            render: function (args) {
              const label = args.label,
                value = args.value;
              return label + ': ' + value;
            },
            position: 'outside'
          },
          legend: { position: 'right' },
          responsive: true
        }
       this.isDataAvailableTicket = true;
        this.forceChartRefresh();
        this.spinner.hide();
      },
      error => {
        this.spinner.hide();
          
        });
  }

  //gender analytics
  GetEventBarGenderAnalytics(eventID) {
    this.spinner.show();
    this.RvCellService.getEventGenderAnalytics(eventID)
      .subscribe(data => {
        
        if ((data.Response.Status == "Success") && (data.Response.Message.Males > 0 || data.Response.Message.Females > 0)) {
          this.chartLabelsGender = ["Male", "Female"];
          this.chartDataGender = [data.Response.Message.Males, data.Response.Message.Females];
          this.chartOptionsGender = {
            pieceLabel: {
              render: function (args) {
                const label = args.label,
                  value = args.value;
                return label + ': ' + value;
              },
              position: 'outside'
            },
            legend: { position: 'right' },
            responsive: true
          }
          this.isDataAvailableGender = true;
          this.spinner.hide();
          //this.forceChartRefresh();
        }
      },
      error => {
        this.spinner.hide();
          
        });
  }
  
  ageGroup: Array<string> = [];

   //age group analytics
  GetEventLineAnalytics(eventID) {
    this.spinner.show();
    this.RvCellService.GetAgeGroupAnalytics(eventID)
      .subscribe(data => {
        
        if (data.Response.Status == "Success") {
          if (data.Response.Message.lstCounts.length > 0) {
            var d = data.Response.Message.lstCounts.map(o => {
              this.lineChartData = [
                { data: [o.Age25, o.Age50, o.Age75, o.Age100, o.Age125], label: '0-20 Age' },
                { data: [o.Age25, o.Age50, o.Age75, o.Age100, o.Age125], label: '25-50 Age' },
                { data: [o.Age25, o.Age50, o.Age75, o.Age100, o.Age125], label: '50-75 Age' },
                { data: [o.Age25, o.Age50, o.Age75, o.Age100, o.Age125], label: '75-100 Age' },
                { data: [o.Age25, o.Age50, o.Age75, o.Age100, o.Age125], label: 'Above 100' }

              ];
            });

            this.lineChartLabels = ['0-20 Age', '25-50 Age', '50-75 Age', '75-100 Age', 'Above 100'];
            this.lineChartOptions = {
              responsive: true,
              legend: { position: 'right' },
              scaleShowVerticalLines: false,

              scaleShowValues: true,
              scaleValuePaddingX: 10,
              scaleValuePaddingY: 10,
              animation: {
                onComplete: function () {
                  var chartInstance = this.chart,
                    ctx = chartInstance.ctx;
                  ctx.textAlign = 'center';
                  ctx.textBaseline = 'bottom';
                  this.data.datasets.forEach(function (dataset, i) {
                    var meta = chartInstance.controller.getDatasetMeta(i);
                    meta.data.forEach(function (bar, index) {
                      var data = dataset.data[index];
                      ctx.fillText(data, bar._model.x, bar._model.y - 5);
                    });
                  });
                }
              }
            };
            this.isDataAvailableAge = true;
            this.spinner.hide();
          }
        }
      },
      error => {
        this.spinner.hide();
        //this.loading = false;
      });
  }


  GetEventBarAnalytics(eventID) {
    this.spinner.show();
    this.RvCellService.getEventTicketSummaryAnalytics(eventID)
      .subscribe(data => {
        
        if (data.Response.Message.lstCounts.length > 0) {
          var d = data.Response.Message.lstCounts.map(o => {
            if (o.Category !== null) {
              this.barLabels.push(o.Category);
            }
            if (o.TotalTickets !== null) {
              this.ticketTotal.push(o.TotalTickets);
            }
            if (o.Sold !== null) {
              this.ticketsoldTotal.push(o.Sold);
            }
            if (o.Attendees !== null) {
              this.ticketattendedTotal.push(o.Attendees);
            }

          });

          this.barData = [
            {
              label: 'Total',
              data: this.ticketTotal
            },
            {
              label: 'Sold',
              data: this.ticketsoldTotal
            },
            {
              label: 'Attended',
              data: this.ticketattendedTotal
            }
          ];
          this.barOptions = {
            pieceLabel: {
              render: function (args) {
                const label = args.label,
                  value = args.value;
                return label + ': ' + value;
              },
              position: 'outside'
            },
            legend: { position: 'right' },
            responsive: true,
            scaleShowVerticalLines: false,

            scaleShowValues: true,
            scaleValuePaddingX: 10,
            scaleValuePaddingY: 10,
            animation: {
              onComplete: function () {
                var chartInstance = this.chart,
                  ctx = chartInstance.ctx;
                ctx.textAlign = 'center';
                ctx.textBaseline = 'bottom';
                this.data.datasets.forEach(function (dataset, i) {
                  var meta = chartInstance.controller.getDatasetMeta(i);
                  meta.data.forEach(function (bar, index) {
                    var data = dataset.data[index];
                    ctx.fillText(data, bar._model.x, bar._model.y - 5);
                  });
                });
              }
            }
          }
          this.isDataAvailable = true;
          this.spinner.hide();
         // this.forceChartRefresh();
        }
      },
      error => {
        this.spinner.hide();
          //this.loading = false;
        });
  }

  GetEventUsers(eventID) {
    this.RvCellService.getEventUsers(eventID)
      .subscribe(data => {
        
        this.eventUsers = data;
      },
        error => {
          this.loading = false;
        });
  }
}


