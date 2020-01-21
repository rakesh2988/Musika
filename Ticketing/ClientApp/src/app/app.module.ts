import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';

import { RVCellService } from './service/service';
import { LoginComponent } from './login/login.component';

import { RegisterComponent } from './Users/register/register.component';
import { ForgetPasswordComponent } from './Users/forgetpassword/forgetpassword.component';
import { ChangePasswordComponent } from './Users/changepassword/changepassword.component';
import { StaffEventCreationComponent } from './StaffEvents/staffeventcreation/staffeventcreation.component';
import { StaffEventFeaturesComponent } from './staffevents/staffeventfeatures/staffeventfeatures.component';
import { StaffEventListingComponent } from './staffevents/staffeventlisting/staffeventlisting.component';
import { TicketingEventCreationComponent } from './ticketingevents/ticketingeventcreation/ticketingeventcreation.component';
import { TicketingEventFeaturesComponent } from './ticketingevents/ticketingeventfeatures/ticketingeventfeatures.component';
import { TicketingEventListingComponent } from './ticketingevents/ticketingeventlisting/ticketingeventlisting.component';
import { UserTicketingEventCreationComponent } from './userticketingevent/userticketingeventcreation/userticketingeventcreation.component';
import { UserTicketingEventFeaturesComponent } from './userticketingevent/userTicketingeventfeatures/userticketingeventfeatures.component';
import { UserTicketingEventListingComponent } from './userticketingevent/userTicketingeventlisting/userTicketingeventlisting.component';
import { NgMultiSelectDropDownModule } from 'ng-multiselect-dropdown';

import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';

import { TicketGenerationComponent } from './ticketgeneration/ticketgeneration.component';
import { AuthGuard } from './service/AuthGuard';

import { HeaderService } from './header/nav-menu.service';
import { DashboardComponent } from './EventAdmin/dashboard/dashboard.component';
import { EventCreationComponent } from './EventAdmin/events/eventcreation/eventcreation.component';
//import { EventDetailComponent } from './EventAdmin/events/eventdetail/eventdetail.component';
import { EventListingComponent } from './EventAdmin/events/eventlisting/eventlisting.component';
import { routingModule } from './app.routes';
import { DashboardModule } from './EventAdmin/dashboard/dashboard.module';
import { SidebarNavComponent } from './EventAdmin/sidebarNav/sidebarNav.component';
import { AdminLayoutComponent } from './_layout/admin-layout/admin-layout.component';
import { CreditCardComponent } from './EventAdmin/credit/credit.component';
import { CKEditorModule } from 'ngx-ckeditor';
import { MyDatePickerModule } from 'mydatepicker';
import { AmazingTimePickerModule } from 'amazing-time-picker';
import { TicketsComponent } from './EventAdmin/events/tickets/tickets.component';
import { ChartsModule } from 'ng2-charts';
import 'chart.piecelabel.js';
import { NgxSpinnerModule } from 'ngx-spinner';
import { TicketsAnalyticsComponent } from './EventAdmin/events/ticketsAnalytics/ticketsAnalytics.component';
import { StaffCreationComponent } from './EventAdmin/staff/staffcreation/staffcreation.component';
import { StaffListingComponent } from './EventAdmin/staff/stafflisting/stafflisting.component';
import { HashLocationStrategy, LocationStrategy } from '@angular/common';
import { ROUTER_PROVIDERS } from '@angular/router/src/router_module';
//import { EventStaffListingComponent } from './EventAdmin/staff/eventstafflisting/eventstafflisting.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { FilterPipe } from './service/filter';
import { ContactComponent } from './contact/contact.component';
import { AboutComponent } from './about/about.component';
import { PrivacyComponent } from './privacy/privacy.component';
import { TermsComponent } from './terms/terms.component';
import { SelectDropDownModule } from 'ngx-select-dropdown'
import { NgxSelectModule, INgxSelectOptions } from 'ngx-select-ex';
//import { Ng5BreadcrumbModule, BreadcrumbService } from 'ng5-breadcrumb';
import { BreadcrumbComponent } from './shared/breadcrumb/breadcrumb.component';
import { CouponComponent } from './EventAdmin/coupon/create/coupon.component';
import { CouponListingComponent } from './EventAdmin/coupon/listcoupon/couponlisting.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    LoginComponent,
    DashboardComponent,
    AdminLayoutComponent,
    RegisterComponent,
    ForgetPasswordComponent,
    ChangePasswordComponent,
    StaffEventCreationComponent,
    StaffEventFeaturesComponent,
    StaffEventListingComponent,
    TicketingEventCreationComponent,
    TicketingEventFeaturesComponent,
    TicketingEventListingComponent,
    UserTicketingEventCreationComponent,
    UserTicketingEventFeaturesComponent,
    UserTicketingEventListingComponent,
    EventCreationComponent,
    //EventDetailComponent,
    EventListingComponent,
    HeaderComponent,
    FooterComponent,
    TicketGenerationComponent,
    TicketsAnalyticsComponent,
    SidebarNavComponent,
    TicketsComponent,
    StaffCreationComponent,
    StaffListingComponent,
    CreditCardComponent,
    //EventStaffListingComponent,
    FilterPipe,
    ContactComponent,
    AboutComponent,
    PrivacyComponent,
    BreadcrumbComponent,
    TermsComponent,
    CouponListingComponent,
    CouponComponent


  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    DashboardModule,
    ReactiveFormsModule,
    HttpModule,
    MyDatePickerModule,
    AmazingTimePickerModule,
    NgMultiSelectDropDownModule.forRoot(),
    NgxSpinnerModule,
    CKEditorModule,
    routingModule,
    ChartsModule,
    NgxPaginationModule, SelectDropDownModule, NgxSelectModule
    //,Ng5BreadcrumbModule.forRoot()

  ],
  exports: [FormsModule, ReactiveFormsModule, FilterPipe],
  providers: [RVCellService, HeaderService, { provide: LocationStrategy, useClass: HashLocationStrategy }],
  bootstrap: [AppComponent]
})
export class AppModule {
    }
