"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var router_1 = require("@angular/router");
var home_component_1 = require("./home/home.component");
var login_component_1 = require("./login/login.component");
var register_component_1 = require("./Users/register/register.component");
var forgetpassword_component_1 = require("./Users/forgetpassword/forgetpassword.component");
var changepassword_component_1 = require("./Users/changepassword/changepassword.component");
var eventcreation_component_1 = require("./EventAdmin/events/eventcreation/eventcreation.component");
var eventdetail_component_1 = require("./EventAdmin/events/eventdetail/eventdetail.component");
var eventlisting_component_1 = require("./EventAdmin/events/eventlisting/eventlisting.component");
var header_component_1 = require("./header/header.component");
var ticketgeneration_component_1 = require("./ticketgeneration/ticketgeneration.component");
var footer_component_1 = require("./footer/footer.component");
var admin_layout_component_1 = require("./_layout/admin-layout/admin-layout.component");
var tickets_component_1 = require("./EventAdmin/events/tickets/tickets.component");
var ticketsAnalytics_component_1 = require("./EventAdmin/events/ticketsAnalytics/ticketsAnalytics.component");
var staffcreation_component_1 = require("./EventAdmin/staff/staffcreation/staffcreation.component");
var stafflisting_component_1 = require("./EventAdmin/staff/stafflisting/stafflisting.component");
var credit_component_1 = require("./EventAdmin/credit/credit.component");
var eventstafflisting_component_1 = require("./EventAdmin/staff/eventstafflisting/eventstafflisting.component");
var routes = [
    { path: '', component: home_component_1.HomeComponent, pathMatch: 'full' },
    { path: 'login', component: login_component_1.LoginComponent },
    { path: 'register', component: register_component_1.RegisterComponent },
    //  { path: 'dashboard', component: DashboardComponent },
    { path: 'forgetpassword', component: forgetpassword_component_1.ForgetPasswordComponent },
    { path: 'changepassword', component: changepassword_component_1.ChangePasswordComponent },
    {
        path: '',
        component: admin_layout_component_1.AdminLayoutComponent,
        children: [
            //{ path: 'dashboard', component: DashboardComponent, pathMatch: 'full' },
            { path: 'eventcreation', component: eventcreation_component_1.EventCreationComponent },
            { path: 'event/edit/:id', component: eventcreation_component_1.EventCreationComponent },
            { path: 'event/detail/:id', component: eventdetail_component_1.EventDetailComponent },
            { path: 'tickets', component: tickets_component_1.TicketsComponent },
            { path: 'eventlisting', component: eventlisting_component_1.EventListingComponent },
            { path: 'ticketAnalytics/:id', component: ticketsAnalytics_component_1.TicketsAnalyticsComponent },
            { path: 'staffcreation', component: staffcreation_component_1.StaffCreationComponent },
            { path: 'staff/edit/:id', component: staffcreation_component_1.StaffCreationComponent },
            { path: 'stafflisting', component: stafflisting_component_1.StaffListingComponent },
            { path: 'eventstafflisting/:id', component: eventstafflisting_component_1.EventStaffListingComponent },
            { path: 'creditcard', component: credit_component_1.CreditCardComponent }
        ]
    },
    { path: 'header', component: header_component_1.HeaderComponent },
    { path: 'footer', component: footer_component_1.FooterComponent },
    { path: 'ticketgeneration', component: ticketgeneration_component_1.TicketGenerationComponent }
];
exports.routingModule = router_1.RouterModule.forRoot(routes);
//# sourceMappingURL=app.routes.js.map