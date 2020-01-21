import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RVCellService } from '../service/service';
import { HeaderService } from './nav-menu.service';


@Component({
  selector: 'header-component',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})

export class HeaderComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  isAuthenticate: boolean;
 // isExpanded = false;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public nav: HeaderService,
    private RvCellService: RVCellService) {
      nav.IsAuthentication();
    if (!nav.isAuthenticate) {
      this.router.navigate(['/']);
    }}

  logout() {
    
    this.RvCellService.logout();
    this.nav.IsAuthentication();
    this.router.navigate(['/']);
  }
  //toggle() {
  //  this.isExpanded = !this.isExpanded;
  //}
  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }
}
