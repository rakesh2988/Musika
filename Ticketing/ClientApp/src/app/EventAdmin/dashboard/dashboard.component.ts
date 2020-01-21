import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderService } from '../../header/nav-menu.service';
import { User } from '../../models/usermodel';
import { RVCellService } from '../../service/service';
import { BaseChartDirective } from 'ng2-charts/ng2-charts';
//import { SidebarNavComponent } from '../sidebarNav/sidebarNav.component';

@Component({
  selector: 'dashboard-component',
  templateUrl: './dashboard.component.html'
})

export class DashboardComponent implements OnInit {
  currentUser: User;
  users: User[] = [];
  chartType: string = 'pie';
  chartLabels: Array<any> = [];
  chartData: Array<any> = [];
  chartOptions: any = {
  }
  
  constructor(public nav: HeaderService, private RvCellService: RVCellService) {
    nav.IsAuthentication();
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    this.RvCellService.getTicketsAnalytics()
      .subscribe(data => {
        this.chartLabels = ["Test Event-01", "Test Event", "Test Event-01", "Test Event-02", "Test Event-03", "Test Event-04", "Test Event", "Test Event 2", "Event Title testing", "Test Event - 001", "Test Event - 001", "Test Event - 001", "ghgfh", "Test Event - 001", "jhgj", "New Image testing Event", "testing.....", "testing ticket Type", "imagetesting event", "testing event image", "testing image upload", "fdgfdg", "ggfhgf", "event title 3-10-2018", "sdfdsf"];
        this.chartData = [1105, 1105, 1105, 1105, 1105, 1105, 1165, 1165, 1165, 1165, 1701, 1701, 1701, 1701, 1701, 1165, 1165, 1165, 1165, 1165, 1165, 1165, 1165, 1165, 1165];
        this.chartOptions = {
          pieceLabel: {
            render: function (args) {
              const label = args.label,
                value = args.value;
              return label + ': ' + value;
            },
            position: 'outside'
          }
        }
      },
        error => {
        });
  }

  // events

  ngOnInit() { }
  GetAllEvents() {
    this.RvCellService.getTicketsAnalytics()
      .subscribe(data => {
        console.log(data);
        this.chartLabels = data.events;
        this.chartData = data.counts;
      },
        error => {
          //this.loading = false;
        });
  }
}
