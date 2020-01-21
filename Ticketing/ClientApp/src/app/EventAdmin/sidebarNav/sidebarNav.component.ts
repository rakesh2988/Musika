import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../../models/usermodel';
import { RVCellService } from '../../service/service';
import { HeaderService } from '../../header/nav-menu.service';



@Component({
  selector: 'sidebar-nav',
  templateUrl: './sidebarNav.component.html',
  styleUrls: ['./sidebarNav.component.css']
})

export class SidebarNavComponent {
  currentUser: User;
  users: User[] = [];
  //isdisable: boolean = false;
  errorMessage: any;
  collapsed = true;
 
  constructor(private RvCellService: RVCellService, private router: Router,public nav: HeaderService,) {
   
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
  }
  toggle(): void {
    this.collapsed = !this.collapsed;
  }
}
