using ArtSeeker.SqlHelper;
using Musika.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musika.Repository.SPRepository
{
    public class SpRepository
    {
        private readonly MusikaEntities db;
        private Int16 _RetVal = 0;
        private string _ErrorDescription = "";

        public SpRepository()
        {
            db = new MusikaEntities();
        }

        public Int16 RetVal
        {
            get
            {
                return _RetVal;
            }
            set
            {
                _RetVal = value;
            }
        }

        public string ErrorDescription
        {
            get
            {
                return _ErrorDescription;
            }
            set
            {
                _ErrorDescription = value;
            }
        }

        //Get Followers/Following List
        //public List<GetUserFollows_Result> Sp_GetUserFollow(int vUserID,string vFollowingType,int vLongedInUserID)
        //{
        //    ObjectResult<GetUserFollows_Result> _ObjectResult;
        //    _ObjectResult = db.GetUserFollows(vUserID, vFollowingType, vLongedInUserID);
        //    return _ObjectResult.ToList();
        //}

        #region "Ado.Net"
        public DataSet SpGetDiscoverDetail(Int32 userId, double lat, double lon, int EventLimit)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int, 4, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, userId));
            sCmd.Parameters.Add(new SqlParameter("@Lat", SqlDbType.Decimal, 8, ParameterDirection.Input, false, 18, 6, "", DataRowVersion.Default, lat));
            sCmd.Parameters.Add(new SqlParameter("@Lon", SqlDbType.Decimal, 8, ParameterDirection.Input, false, 18, 6, "", DataRowVersion.Default, lon));
            sCmd.Parameters.Add(new SqlParameter("@EventLImit", SqlDbType.Int, 4, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, EventLimit));
            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetDiscoverDetail", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetDiscoverDetailWithOutUserId(double lat, double lon, int EventLimit)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@Lat", SqlDbType.Decimal, 8, ParameterDirection.Input, false, 18, 6, "", DataRowVersion.Default, lat));
            sCmd.Parameters.Add(new SqlParameter("@Lon", SqlDbType.Decimal, 8, ParameterDirection.Input, false, 18, 6, "", DataRowVersion.Default, lon));
            sCmd.Parameters.Add(new SqlParameter("@EventLImit", SqlDbType.Int, 4, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, EventLimit));
            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetDiscoverDetailWithoutUserId", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpDeleteArtist(Int32 _ArtistID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@ArtistID", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _ArtistID));
            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpDeleteArtist_Admin", SqlCommandType.StoredProcedure, sCmd);
            _ErrorDescription = sqlManager.ErrorDescription;
            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }

            return ds;
        }

        public DataSet SpGetVenueList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetVenueList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetUserList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetUserList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetArtistList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetArtistList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetGenreList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetGenreList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetUserArtistList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetUserArtistList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetEventList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetEventList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetEventDuplicates()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetEventDuplicates_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }


        public DataTable SpGetTicketingInventory(int PageIndex, string EventName, string ArtistName, string GenreName, string SortColumn, string SortOrder)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.Add(new SqlParameter("@PageIndex", SqlDbType.Int, 4, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, PageIndex));
            sCmd.Parameters.Add(new SqlParameter("@EventName", SqlDbType.VarChar, 300, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, EventName));
            sCmd.Parameters.Add(new SqlParameter("@ArtistName", SqlDbType.VarChar, 300, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, ArtistName));
            sCmd.Parameters.Add(new SqlParameter("@GenreName", SqlDbType.VarChar, 300, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, GenreName));
            sCmd.Parameters.Add(new SqlParameter("@SortColumn", SqlDbType.VarChar, 100, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, SortColumn));
            sCmd.Parameters.Add(new SqlParameter("@SortOrder", SqlDbType.VarChar, 100, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, SortOrder));
            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataTable("SpGetTicketInventory_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataTable SpGetTicketingCategory(int PageIndex, string Name, string SortColumn, string SortOrder)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@PageIndex", SqlDbType.Int, 4, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, PageIndex));
            sCmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.VarChar, 500, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, Name));
            sCmd.Parameters.Add(new SqlParameter("@SortColumn", SqlDbType.VarChar, 100, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, SortColumn));
            sCmd.Parameters.Add(new SqlParameter("@SortOrder", SqlDbType.VarChar, 100, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Proposed, SortOrder));
            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataTable("SpGetTicketCategory_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;
            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }
        #endregion

        public DataSet SpGetTicketingEventDuplicates()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketingEventDuplicates_Admin", SqlCommandType.StoredProcedure, sCmd);
            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetTicketingEventList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketingEventList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }
            return ds;
        }

        public DataSet SpGetTicketingUserList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@sError_Description", SqlDbType.VarChar, 200, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Proposed, DBNull.Value));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketingUserList_Admin", SqlCommandType.StoredProcedure, sCmd);

            _ErrorDescription = sqlManager.ErrorDescription;

            if (_ErrorDescription == "")
            {
                _ErrorDescription = sCmd.Parameters["@sError_Description"].Value.ToString();
            }

            return ds;
        }

        // Add Ticketing Event Tickets
        public bool SpAddTicketingEventTickets(Int32 eventID, Int32? noOfTickets)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpGenerateTicketingEventTickets";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventID", eventID);
                sCmd.Parameters.AddWithValue("@NoOfTickets", noOfTickets);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Update Ticketing Event Tickets
        public bool SpUpdateTicketingEventTickets(Int32 eventID, string ticketNumber, Int32 UserId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateTicketingEventTickets";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventID", eventID);
                sCmd.Parameters.AddWithValue("@ticketNumber", ticketNumber);
                sCmd.Parameters.AddWithValue("@UserId", UserId);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        public DataSet SpDashboradSummary(int eventId)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);

            try
            {
                cmd = new SqlCommand("spDashboradSummary", cn);
                cmd.Parameters.Add(new SqlParameter("@EventID", eventId));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                //da.Fill(dt);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                cmd.Dispose();
                cn.Close();
                cn.Dispose();
            }
        }

        public DataSet SpUsersAttendingEvent(int eventId)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            try
            {
                cmd = new SqlCommand("spAttendingUsersList", cn);
                cmd.Parameters.Add(new SqlParameter("@EventID", eventId));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                cmd.Dispose();
                cn.Close();
                cn.Dispose();
            }
        }

        // Get Event Details
        public DataSet SpGetEventDetails(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("spGetTicketingEventDetails", SqlCommandType.StoredProcedure, sCmd);

            return ds;
        }

        // Add New Event
        public string SpAddTicketingEvent(TicketingEventsNew ticket)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateTicketingEvent";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventTitle", ticket.EventTitle);
                sCmd.Parameters.AddWithValue("@EventLocation", ticket.EventLocation);
                sCmd.Parameters.AddWithValue("@VenueName", ticket.VenueName);
                sCmd.Parameters.AddWithValue("@Address1", ticket.Address1);
                sCmd.Parameters.AddWithValue("@Address2", ticket.Address2);
                sCmd.Parameters.AddWithValue("@City", ticket.City);
                sCmd.Parameters.AddWithValue("@State", ticket.State);
                sCmd.Parameters.AddWithValue("@ZipCode", ticket.ZipCode);
                sCmd.Parameters.AddWithValue("@StartDate", ticket.StartDate);
                sCmd.Parameters.AddWithValue("@StartTime", ticket.StartTime);
                sCmd.Parameters.AddWithValue("@EndDate", ticket.EndDate);
                sCmd.Parameters.AddWithValue("@EndTime", ticket.EndTime);

                sCmd.Parameters.AddWithValue("@EventImage", (String.IsNullOrEmpty(ticket.EventImage) ? "" : ticket.EventImage));
                sCmd.Parameters.AddWithValue("@EventDescription", (String.IsNullOrEmpty(ticket.EventDescription) ? "" : ticket.EventDescription));
                sCmd.Parameters.AddWithValue("@OrganizerName", (String.IsNullOrEmpty(ticket.OrganizerName) ? "" : ticket.OrganizerName));
                sCmd.Parameters.AddWithValue("@OrganizerDescription", (String.IsNullOrEmpty(ticket.OrganizerDescription) ? "" : ticket.OrganizerDescription));
                sCmd.Parameters.AddWithValue("@TicketType", ticket.TicketType);
                sCmd.Parameters.AddWithValue("@ListingPrivacy", ticket.ListingPrivacy);
                sCmd.Parameters.AddWithValue("@EventType", ticket.EventType);
                sCmd.Parameters.AddWithValue("@EventTopic", ticket.EventTopic);
                sCmd.Parameters.AddWithValue("@ShowTicketNumbers", ticket.ShowTicketNumbers);
                sCmd.Parameters.AddWithValue("@CreatedBy", ticket.CreatedBy);
                sCmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                sCmd.Parameters.AddWithValue("@ISDELETED", ticket.ISDELETED);
                if (ticket.IsApproved == null)
                {
                    ticket.IsApproved = false;
                }
                else
                {
                    ticket.IsApproved = true;
                }
                sCmd.Parameters.AddWithValue("@IsApproved", ticket.IsApproved);
                sCmd.Parameters.AddWithValue("@NumberOfTickets", ticket.NumberOfTickets);
                sCmd.Parameters.AddWithValue("@ArtistId", ticket.ArtistId);
                sCmd.Parameters.AddWithValue("@StaffId", (ticket.StaffId == null) ? 0 : ticket.StaffId);
                sCmd.Parameters.AddWithValue("@TicketUrl", ticket.TicketUrl);

                sCmd.Parameters.Add("@ctr", SqlDbType.Int).Direction = ParameterDirection.Output;
                sCmd.ExecuteNonQuery();
                string id = sCmd.Parameters["@ctr"].Value.ToString();
                return id;
            }
            catch (Exception ex)
            {
                return "0";
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        //SpUpdateTicketingEvent
        public string SpUpdateTicketingEvent(TicketingEventsNew ticket)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateTicketingEventNew";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventId", ticket.EventID);
                sCmd.Parameters.AddWithValue("@EventTitle", ticket.EventTitle);
                sCmd.Parameters.AddWithValue("@EventLocation", ticket.EventLocation);
                sCmd.Parameters.AddWithValue("@VenueName", ticket.VenueName);
                sCmd.Parameters.AddWithValue("@Address1", ticket.Address1);
                sCmd.Parameters.AddWithValue("@Address2", ticket.Address2);
                sCmd.Parameters.AddWithValue("@City", ticket.City);
                sCmd.Parameters.AddWithValue("@State", ticket.State);
                sCmd.Parameters.AddWithValue("@ZipCode", ticket.ZipCode);
                sCmd.Parameters.AddWithValue("@StartDate", ticket.StartDate);
                sCmd.Parameters.AddWithValue("@StartTime", ticket.StartTime);
                sCmd.Parameters.AddWithValue("@EndDate", ticket.EndDate);
                sCmd.Parameters.AddWithValue("@EndTime", ticket.EndTime);

                sCmd.Parameters.AddWithValue("@EventImage", (String.IsNullOrEmpty(ticket.EventImage) ? "" : ticket.EventImage));
                sCmd.Parameters.AddWithValue("@EventDescription", (String.IsNullOrEmpty(ticket.EventDescription) ? "" : ticket.EventDescription));
                sCmd.Parameters.AddWithValue("@OrganizerName", (String.IsNullOrEmpty(ticket.OrganizerName) ? "" : ticket.OrganizerName));
                sCmd.Parameters.AddWithValue("@OrganizerDescription", (String.IsNullOrEmpty(ticket.OrganizerDescription) ? "" : ticket.OrganizerDescription));
                sCmd.Parameters.AddWithValue("@TicketType", ticket.TicketType);
                sCmd.Parameters.AddWithValue("@ListingPrivacy", ticket.ListingPrivacy);
                sCmd.Parameters.AddWithValue("@EventType", ticket.EventType);
                sCmd.Parameters.AddWithValue("@EventTopic", ticket.EventTopic);
                sCmd.Parameters.AddWithValue("@ShowTicketNumbers", ticket.ShowTicketNumbers);
                sCmd.Parameters.AddWithValue("@IsApproved", ticket.IsApproved);
                sCmd.Parameters.AddWithValue("@NumberOfTickets", ticket.NumberOfTickets);
                sCmd.Parameters.AddWithValue("@ArtistId", ticket.ArtistId);
                sCmd.Parameters.AddWithValue("@StaffId", (ticket.StaffId == null) ? 0 : ticket.StaffId);
                sCmd.Parameters.AddWithValue("@TicketUrl", ticket.TicketUrl);

                sCmd.Parameters.Add("@ctr", SqlDbType.Int).Direction = ParameterDirection.Output;
                sCmd.ExecuteNonQuery();
                string id = sCmd.Parameters["@ctr"].Value.ToString();
                return id;
            }
            catch (Exception)
            {
                return "0";
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }


        //SpUpdateTicketingEvent in temporary table
        public string SpUpdateTicketingEventTemp(TicketingEventsNew ticket)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpInsertTicketingEventNewInTemp";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventId", ticket.EventID);
                sCmd.Parameters.AddWithValue("@EventTitle", ticket.EventTitle);
                sCmd.Parameters.AddWithValue("@EventLocation", ticket.EventLocation);
                sCmd.Parameters.AddWithValue("@VenueName", ticket.VenueName);
                sCmd.Parameters.AddWithValue("@Address1", ticket.Address1);
                sCmd.Parameters.AddWithValue("@Address2", ticket.Address2);
                sCmd.Parameters.AddWithValue("@City", ticket.City);
                sCmd.Parameters.AddWithValue("@State", ticket.State);
                sCmd.Parameters.AddWithValue("@ZipCode", ticket.ZipCode);
                sCmd.Parameters.AddWithValue("@StartDate", ticket.StartDate);
                sCmd.Parameters.AddWithValue("@StartTime", ticket.StartTime);
                sCmd.Parameters.AddWithValue("@EndDate", ticket.EndDate);
                sCmd.Parameters.AddWithValue("@EndTime", ticket.EndTime);

                sCmd.Parameters.AddWithValue("@EventImage", (String.IsNullOrEmpty(ticket.EventImage) ? "" : ticket.EventImage));
                sCmd.Parameters.AddWithValue("@EventDescription", (String.IsNullOrEmpty(ticket.EventDescription) ? "" : ticket.EventDescription));
                sCmd.Parameters.AddWithValue("@OrganizerName", (String.IsNullOrEmpty(ticket.OrganizerName) ? "" : ticket.OrganizerName));
                sCmd.Parameters.AddWithValue("@OrganizerDescription", (String.IsNullOrEmpty(ticket.OrganizerDescription) ? "" : ticket.OrganizerDescription));
                sCmd.Parameters.AddWithValue("@TicketType", ticket.TicketType);
                sCmd.Parameters.AddWithValue("@ListingPrivacy", ticket.ListingPrivacy);
                sCmd.Parameters.AddWithValue("@EventType", ticket.EventType);
                sCmd.Parameters.AddWithValue("@EventTopic", ticket.EventTopic);
                sCmd.Parameters.AddWithValue("@ShowTicketNumbers", ticket.ShowTicketNumbers);
                sCmd.Parameters.AddWithValue("@IsApproved", ticket.IsApproved);
                sCmd.Parameters.AddWithValue("@NumberOfTickets", ticket.NumberOfTickets);
                sCmd.Parameters.AddWithValue("@ArtistId", ticket.ArtistId);
                sCmd.Parameters.AddWithValue("@StaffId", (ticket.StaffId == null) ? 0 : ticket.StaffId);
                sCmd.Parameters.AddWithValue("@TicketUrl", ticket.TicketUrl);
                sCmd.Parameters.AddWithValue("@CreatedBy", ticket.CreatedBy);
                sCmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                sCmd.Parameters.AddWithValue("@ISDELETED", ticket.ISDELETED);
                sCmd.Parameters.Add("@ctr", SqlDbType.Int).Direction = ParameterDirection.Output;
                sCmd.CommandTimeout = 5000;
                sCmd.ExecuteNonQuery();
                string id = sCmd.Parameters["@ctr"].Value.ToString();
                return id;
            }
            catch (Exception ex)
            {
                return "0";
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Generate Tickets for An Event
        public bool SpGenerateTicketingEventTickets(int EventID, int UserId, string TicketType, int NoOfTickets)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpGenerateTicketingEventTickets";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventID", EventID);
                sCmd.Parameters.AddWithValue("@UserId", UserId);
                sCmd.Parameters.AddWithValue("@TicketType", TicketType);
                sCmd.Parameters.AddWithValue("@NoOfTickets", NoOfTickets);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Add Event Summary
        public bool SpUpdateTicketingEventTicketSummary(int EventID, string TicketCategory, string TicketType, decimal Cost, int Quantity, int CountryId, string Currency, string ServiceFee, string Tax, string RefundPolicy, string stdate, string endadte)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateTicketingEventTicketSummary";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventID", EventID);
                if (!String.IsNullOrEmpty(TicketCategory))
                {
                    sCmd.Parameters.AddWithValue("@TicketCategory", TicketCategory);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@TicketCategory", String.Empty);
                }
                if (!String.IsNullOrEmpty(TicketType))
                {
                    sCmd.Parameters.AddWithValue("@TicketType", TicketType);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@TicketType", String.Empty);
                }
                if (!String.IsNullOrEmpty(Cost.ToString()))
                {
                    sCmd.Parameters.AddWithValue("@Cost", Cost);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Cost", 0);
                }
                if (!String.IsNullOrEmpty(Quantity.ToString()))
                {
                    sCmd.Parameters.AddWithValue("@Quantity", Quantity);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Quantity", 0);
                }
                if (!String.IsNullOrEmpty(CountryId.ToString()))
                {
                    sCmd.Parameters.AddWithValue("@CountryId", CountryId);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@CountryId", 0);
                }
                if (!String.IsNullOrEmpty(Currency))
                {
                    sCmd.Parameters.AddWithValue("@Currency", Currency);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Currency", String.Empty);
                }

                if (!String.IsNullOrEmpty(ServiceFee))
                {
                    sCmd.Parameters.AddWithValue("@ServiceFee", ServiceFee);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@ServiceFee", String.Empty);
                }

                if (!String.IsNullOrEmpty(Tax))
                {
                    sCmd.Parameters.AddWithValue("@Tax", Tax);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Tax", String.Empty);
                }
                if (!String.IsNullOrEmpty(RefundPolicy))
                {
                    sCmd.Parameters.AddWithValue("@RefundPolicy", RefundPolicy);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@RefundPolicy", String.Empty);
                }
                if (!String.IsNullOrEmpty(stdate))
                {
                    sCmd.Parameters.AddWithValue("@PackageStartDate", stdate);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@PackageStartDate", String.Empty);
                }
                if (!String.IsNullOrEmpty(endadte))
                {
                    sCmd.Parameters.AddWithValue("@PackageEndDate", endadte);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@PackageEndDate", String.Empty);
                }

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }


        public bool SpUpdateTicketingEventTicketSummaryTemp(int EventID, string TicketCategory, string TicketType, decimal Cost, int Quantity, int CountryId, string Currency, string ServiceFee, string Tax, string RefundPolicy, string stdate, string endadte)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateTicketingEventTicketSummaryTemp";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventID", EventID);
                if (!String.IsNullOrEmpty(TicketCategory))
                {
                    sCmd.Parameters.AddWithValue("@TicketCategory", TicketCategory);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@TicketCategory", String.Empty);
                }
                if (!String.IsNullOrEmpty(TicketType))
                {
                    sCmd.Parameters.AddWithValue("@TicketType", TicketType);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@TicketType", String.Empty);
                }
                if (!String.IsNullOrEmpty(Cost.ToString()))
                {
                    sCmd.Parameters.AddWithValue("@Cost", Cost);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Cost", 0);
                }
                if (!String.IsNullOrEmpty(Quantity.ToString()))
                {
                    sCmd.Parameters.AddWithValue("@Quantity", Quantity);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Quantity", 0);
                }
                if (!String.IsNullOrEmpty(CountryId.ToString()))
                {
                    sCmd.Parameters.AddWithValue("@CountryId", CountryId);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@CountryId", 0);
                }
                if (!String.IsNullOrEmpty(Currency))
                {
                    sCmd.Parameters.AddWithValue("@Currency", Currency);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Currency", String.Empty);
                }
                if (!String.IsNullOrEmpty(ServiceFee))
                {
                    sCmd.Parameters.AddWithValue("@ServiceFee", ServiceFee);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@ServiceFee", String.Empty);
                }
                if (!String.IsNullOrEmpty(Tax))
                {
                    sCmd.Parameters.AddWithValue("@Tax", Tax);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Tax", String.Empty);
                }
                if (!String.IsNullOrEmpty(RefundPolicy))
                {
                    sCmd.Parameters.AddWithValue("@RefundPolicy", RefundPolicy);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@RefundPolicy", String.Empty);
                }
                if (!String.IsNullOrEmpty(stdate))
                {
                    sCmd.Parameters.AddWithValue("@PackageStartDate", stdate);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@PackageStartDate", String.Empty);
                }
                if (!String.IsNullOrEmpty(endadte))
                {
                    sCmd.Parameters.AddWithValue("@PackageEndDate", endadte);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@PackageEndDate", String.Empty);
                }

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Delete Ticketing Event Ticket Summary
        public bool SpDeleteTicketingEventTicketSummary(int EventId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpDeleteTicketingEventTicketSummary";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventID", EventId);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Delete Ticketing Event Ticket Summary for temp table
        public bool SpDeleteTicketingEventTicketSummaryTemp(int EventId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpDeleteTicketingEventTicketSummaryTemp";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventID", EventId);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        //Delete Ticketing Event Satff
        public bool SpDeleteTicketingStaff(int EventId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpDeleteTicketingEventStaff";
                cn.Open();

                sCmd.Parameters.AddWithValue("@EventID", EventId);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Get Summary of Tickets for An Event
        public DataSet SpGetTicketsSummaryForAnEvent(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketsSummaryForAnEvent", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet ShiftDatetoOriginalTables(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("ShiftDatetoOriginalTables", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // Get Users List for An Event
        public DataSet SpGetUsersListForAnEvent(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketingEventUsers", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet SpSearchAttendes(Int32 _EventID, string SearchBy)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));
            //if (!String.IsNullOrEmpty(SearchBy))
            //{
            sCmd.Parameters.AddWithValue("@SearchBy", SearchBy);
            //}
            var ds = sqlManager.ExecuteDataSet("SpSearchAttendes", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // Get Users List for An Event
        public DataSet SpGetTicketingEventUsersToSendEmail(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketingEventUsersToSendEmail", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet SpSearchUsersListForAnEvent(Int32 _EventID, String searchBy)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            if (!String.IsNullOrEmpty(searchBy))
            {
                sCmd.Parameters.AddWithValue("@SearchBy", searchBy);
            }

            var ds = sqlManager.ExecuteDataSet("SpSearchTicketingEventUsers", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public bool CheckWhetherAlreadyScanned(int EventID, string TicketNumber, string TicketType, int UserId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpCheckWhetherAlreadyScanned";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventId", EventID);
                sCmd.Parameters.AddWithValue("@TicketNumber", TicketNumber);
                sCmd.Parameters.AddWithValue("@TicketType", TicketType);
                sCmd.Parameters.AddWithValue("@UserId", UserId);
                var res = sCmd.ExecuteScalar();
                if (Convert.ToInt32(res) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Update Ticket Status on Scan
        public string SpUpdateTicketingStatus(int EventID, string TicketNumber, string TicketType, int UserId)
        {
            string result = string.Empty;
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            if (CheckWhetherAlreadyScanned(EventID, TicketNumber, TicketType, UserId) == false)
            {
                SqlCommand sCmd = new SqlCommand();
                try
                {
                    sCmd.CommandType = CommandType.StoredProcedure;
                    sCmd.Connection = cn;
                    sCmd.CommandText = "SpUpdateTicketingEventTicketStatus";
                    cn.Open();
                    sCmd.Parameters.AddWithValue("@EventID", EventID);
                    sCmd.Parameters.AddWithValue("@TicketNumber", TicketNumber);
                    sCmd.Parameters.AddWithValue("@TicketType", TicketType);
                    sCmd.Parameters.AddWithValue("@UserId", UserId);
                    sCmd.Parameters.AddWithValue("@Status", "Present");
                    sCmd.ExecuteNonQuery();
                    result = "Scanned Successfully";
                }
                catch (Exception)
                {
                    result = "Unexpected Error";
                }
                finally
                {
                    if (cn.State == ConnectionState.Open)
                    {
                        cn.Close();
                    }
                    sCmd.Dispose();
                    cn.Dispose();
                }
            }
            else
            {
                result = "Ticket Already Scanned";
            }
            return result;
        }

        // Get Number of Males and Females Attending An Event
        public DataSet SpGetNumberOfMalesFemales(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("GetNumberOfMalesFemales", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // Get Ticket Stats Summary for Event
        public DataSet SpGetTicketSummaryStats(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("GetTicketSummaryStats", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // Get count of users going
        public DataSet SpGetUserGoingCount(Int32 _UserID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _UserID));

            var ds = sqlManager.ExecuteDataSet("GetUserGoingCount", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet SpGetSummary(int TicketingEventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@TicketingEventID", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, TicketingEventID));

            var ds = sqlManager.ExecuteDataSet("getsummary", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // view your friends plan based on tourdate table
        public DataSet SpViewYourFriendsPlan(Int32 _TourID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@tourId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _TourID));

            var ds = sqlManager.ExecuteDataSet("ViewYourFriendsPlan", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // view your friends plan based on ticketingeventsticketconfirmation table
        public DataSet SpViewYourFriendsPlanNew(Int32 UserID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, UserID));

            var ds = sqlManager.ExecuteDataSet("ViewYourFriendsPlanNewByUserID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // view your friends going plan based on tourdate table
        public DataSet SpViewYourFriendsGoingPlan(Int32 _UserID, Int32 _TourID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _UserID));
            sCmd.Parameters.Add(new SqlParameter("@tourId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _TourID));

            var ds = sqlManager.ExecuteDataSet("ViewYourFriendsGoingPlan", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // view your friends going plan based on ticketingeventsticketconfirmation table
        public DataSet SpViewYourFriendsGoingPlanNew(Int32 _UserID, Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@eventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));
            sCmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _UserID));

            var ds = sqlManager.ExecuteDataSet("ViewYourFriendsGoingPlanNew", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        // Get Ticket Stats Summary for Event by Gender
        public DataSet SpGetTicketSummaryStatsByGender(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("GetTicketSummaryStatsByGender", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // Get Ticket Stats by Age Group Summary for Event
        public DataSet SpGetTicketSummaryByAgeGroupStats(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("SpGetTicketSummaryByAgeGroupStats", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        // Update Staff Member
        public bool SpUpdateTicketingStaff(Int32 UserId, string Email, string Password, string Address, string City, string State, string Country, string PostalCode, string PhoneNumber, string username)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateTicketingStaff";
                cn.Open();
                sCmd.Parameters.AddWithValue("@UserId", UserId);
                sCmd.Parameters.AddWithValue("@Email", Email);
                sCmd.Parameters.AddWithValue("@Password", Password);

                sCmd.Parameters.AddWithValue("@Address", Address);
                sCmd.Parameters.AddWithValue("@City", City);
                sCmd.Parameters.AddWithValue("@State", State);
                sCmd.Parameters.AddWithValue("@Country", Country);
                sCmd.Parameters.AddWithValue("@PostalCode", PostalCode);
                sCmd.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                sCmd.Parameters.AddWithValue("@UserName", username);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        public bool SpUpdateTourData(Int32? ArtistId, string Venuename, DateTime? startdate, string EventName, Int32 EventId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "UpdateTourData";
                cn.Open();
                sCmd.Parameters.AddWithValue("@ArtistID", ArtistId);
                sCmd.Parameters.AddWithValue("@Venuename", Venuename);
                sCmd.Parameters.AddWithValue("@Tour_Utcdate", startdate);

                sCmd.Parameters.AddWithValue("@EventName", EventName);
                sCmd.Parameters.AddWithValue("@TicketingEventID", EventId);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Show Details of Ticketing Events
        public DataSet SpGetTicketingEventDetails(Int32 _EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            sCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.BigInt, 8, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, _EventID));

            var ds = sqlManager.ExecuteDataSet("spGetTicketingEventDetailsNew", SqlCommandType.StoredProcedure, sCmd);

            return ds;
        }

        // Add Credit Card Details
        public bool SpAddCreditCardDetails(CreditCardDetails details)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpAddCreditCardDetails";
                cn.Open();
                sCmd.Parameters.AddWithValue("@Auth1", details.Auth1);
                sCmd.Parameters.AddWithValue("@Auth2", details.Auth2);
                sCmd.Parameters.AddWithValue("@MerchantId", details.MerchantId);
                sCmd.Parameters.AddWithValue("@UserId", details.UserId);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }


        // Add Credit Card Details
        public bool SpAddContactDetails(Contact details)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpAddContactDetails";
                cn.Open();
                sCmd.Parameters.AddWithValue("@Name", details.Name);
                sCmd.Parameters.AddWithValue("@Email", details.Email);
                sCmd.Parameters.AddWithValue("@Address", details.Address);
                sCmd.Parameters.AddWithValue("@Message", details.Message);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Get Ticketing Events for Musika
        public DataSet SpGetTicketingEventsForMusika()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();

            var ds = sqlManager.ExecuteDataSet("SpGetTicketingEventsForMusika", SqlCommandType.StoredProcedure, sCmd);

            return ds;
        }

        // Update Ticketing Event Staff
        public bool SpUpdateTicketingEventStaff(int eventID, int StaffId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateEventStaff";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventID", eventID);
                sCmd.Parameters.AddWithValue("@StaffId", StaffId);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Update Ticketing Event Staff
        public bool SpUpdateTicketingEventStaffTemp(int eventID, int StaffId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateEventStaffTemp";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventID", eventID);
                sCmd.Parameters.AddWithValue("@StaffId", StaffId);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }


        // Update Ticketing Event Staff
        public bool SpAddTicketingEventStaff(int eventID, int StaffId)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpAddEventStaff";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventID", eventID);
                sCmd.Parameters.AddWithValue("@StaffId", StaffId);

                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Get Tour Details by TourDateId
        public DataSet SpGetEventDetailsByTourDateId(int tourDateId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TourDateId", tourDateId);

            var ds = sqlManager.ExecuteDataSet("SpGetEventDetailsByTourDateId", SqlCommandType.StoredProcedure, sCmd);

            return ds;
        }

        // Get all temp table detail 
        public Dictionary<string, string> SpGetEventTicketStaffDetails(int eventId)
        {
            Dictionary<string, string> lstData = new Dictionary<string, string>();
            var sqlManager = new SqlManager();
            // Original Data

            var sCmdOrg = new SqlCommand();
            sCmdOrg.Parameters.AddWithValue("@EventId", eventId);
            sCmdOrg.Parameters.AddWithValue("@type", "Original");

            var dsOrg = sqlManager.ExecuteDataSet("SpGetEventTicketStaffDetails", SqlCommandType.StoredProcedure, sCmdOrg);

            //temp data
            var sCmdtemp = new SqlCommand();
            sCmdtemp.Parameters.AddWithValue("@EventId", eventId);
            sCmdtemp.Parameters.AddWithValue("@type", "Temp");

            var dsTemp = sqlManager.ExecuteDataSet("SpGetEventTicketStaffDetails", SqlCommandType.StoredProcedure, sCmdtemp);

            if (dsOrg.Tables[0].Rows.Count > 0 && dsTemp.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsOrg.Tables[0].Rows.Count; i++)
                {
                    for (int j = 0; j < dsOrg.Tables[0].Columns.Count; j++)
                    {
                        if (dsOrg.Tables[0].Rows[i].ItemArray[j].ToString() != dsTemp.Tables[0].Rows[i].ItemArray[j].ToString())
                        {
                            //loadDT.Columns
                            lstData[dsOrg.Tables[0].Columns[j].ColumnName] = dsOrg.Tables[0].Rows[i].ItemArray[j].ToString() + "#" + dsTemp.Tables[0].Rows[i].ItemArray[j].ToString();
                        }
                        //if (dsOrg.Tables[0].Rows[i].ItemArray[0].ToString() != dsTemp.Tables[0].Rows[i].ItemArray[0].ToString())
                        //{
                        //    lstData[dsOrg.Tables[0].Rows[i].ToString()] = dsTemp.Tables[0].Rows[i].ToString();
                        //}
                    }
                }
            }

            return lstData;
        }

        // Save Ticket Details
        public bool SpAddTicketingEventTicketConfirmation(TicketingEventTicketConfirmation details)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpAddTicketingEventTicketConfirmation";
                cn.Open();
                sCmd.Parameters.AddWithValue("@EventID", details.EventID);
                sCmd.Parameters.AddWithValue("@UserID", details.UserID);

                if (details.Dob.ToString().Contains("0001"))
                {
                    sCmd.Parameters.AddWithValue("@Dob", DateTime.Now);
                }
                else
                {
                    sCmd.Parameters.AddWithValue("@Dob", details.Dob ?? DateTime.Now);
                }
                sCmd.Parameters.AddWithValue("@Gender", details.Gender ?? "Male");
                sCmd.Parameters.AddWithValue("@Address", details.Address ?? string.Empty);
                sCmd.Parameters.AddWithValue("@City", details.City ?? string.Empty);
                sCmd.Parameters.AddWithValue("@State", details.State ?? string.Empty);
                sCmd.Parameters.AddWithValue("@Country", details.Country ?? string.Empty);
                sCmd.Parameters.AddWithValue("@PostalCode", details.PostalCode ?? string.Empty);
                sCmd.Parameters.AddWithValue("@Email", details.Email ?? string.Empty);
                sCmd.Parameters.AddWithValue("@PhoneNumber", details.PhoneNumber ?? string.Empty);
                sCmd.Parameters.AddWithValue("@TicketNumber", details.TicketNumber);
                sCmd.Parameters.AddWithValue("@TicketType", details.TicketType);
                sCmd.Parameters.AddWithValue("@Mode", details.Mode ?? string.Empty);
                sCmd.Parameters.AddWithValue("@TicketSerialNumber", details.TicketSerialNumber);
                sCmd.Parameters.AddWithValue("@ScannedTicket", details.ScannedTicket);
                sCmd.Parameters.AddWithValue("@tourDateId", details.TourDateID);
                sCmd.Parameters.AddWithValue("@Quantity", details.Quantity);
                sCmd.Parameters.AddWithValue("@OrderNum ", details.OrderNum);
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        // Get Ticketing Event List
        public DataSet SpGetEventDetailsByTourDateId()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("Sp_TicketingEventList", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet SpTicketingLiveEventListByUserId(int userId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", userId);
            var ds = sqlManager.ExecuteDataSet("Sp_TicketingLiveEventListByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet GetUserByID(int userId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", userId);
            var ds = sqlManager.ExecuteDataSet("spGetUserByID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet SpGetEventDetailsByUserId(int userId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", userId);
            var ds = sqlManager.ExecuteDataSet("Sp_TicketingEventListByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet UserGoingTicketingEventByUserId(int userId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", userId);
            var ds = sqlManager.ExecuteDataSet("Sp_UserGoingTicketingEventByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }


        public DataSet SpGetTicketingEventTicketSummary(int eventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@eventID", eventId);
            var ds = sqlManager.ExecuteDataSet("sp_GetTicketCounts", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetTicketingUserListForAnEvent(int eventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@eventID", eventId);
            var ds = sqlManager.ExecuteDataSet("SP_GetTicketingUserListForAnEvent", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetTicketingEventDetailsByUserId(int UserId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            var ds = sqlManager.ExecuteDataSet("spGetTicketingEventDetailsByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        //
        public DataSet GetTicketingEventDetailsByUserIdAndEventId(int UserId, int EventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            sCmd.Parameters.AddWithValue("@EventId", EventId);
            var ds = sqlManager.ExecuteDataSet("spGetTicketingEventDetailsByUserIdEventId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetEventsList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("SpGetEventList", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetEventsList2()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("SpGetEventList2", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetAritstsByUserId(int UserId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            //var ds = sqlManager.ExecuteDataSet("spGetArtistListByUserId", SqlCommandType.StoredProcedure, sCmd);
            var ds = sqlManager.ExecuteDataSet("spGetAristsList", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet GetAritstsByUserId1(int UserId, string onTour)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            sCmd.Parameters.AddWithValue("@onTour", onTour);
            //var ds = sqlManager.ExecuteDataSet("spGetArtistListByUserId", SqlCommandType.StoredProcedure, sCmd);
            var ds = sqlManager.ExecuteDataSet("spGetAristsList1", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet GetAritstsByUserIdHome(int UserId, string onTour)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            sCmd.Parameters.AddWithValue("@onTour", onTour);
            //var ds = sqlManager.ExecuteDataSet("spGetArtistListByUserId", SqlCommandType.StoredProcedure, sCmd);
            var ds = sqlManager.ExecuteDataSet("spGetAristsListHome", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetEventsListSorted()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("SpGetEventsListSorted", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }



        public bool UpdateCoupon(string couponCode)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateCouponStatus";
                sCmd.Parameters.AddWithValue("@CouponCode", couponCode);
                cn.Open();
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        public decimal GetTicketCost(int tourDateId, string TicketType)
        {
            decimal cost = 0;
            string qry = string.Empty;
            qry = "select round(isnull(Cost,0)*(1-isnull(Discount,0)/100),2) from TicketingEventTicketsSummary inner join TourDate ON ";
            qry += " TicketingEventTicketsSummary.EventID = TourDate.TicketingEventID inner join Coupons on Coupons.EventName=TourDate.EventName ";
            qry += " where TourDateID=" + tourDateId + " and TicketType='" + TicketType + "'";
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlDataAdapter adpt = new SqlDataAdapter(qry, cn);
            DataSet ds = new DataSet();
            adpt.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                cost = Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                cost = 0;
            }
            return cost;
        }

        public decimal GetTicketCost(int tourDateId, string TicketType, string couponCode)
        {
            decimal cost = 0;
            string qry = string.Empty;
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@tourDateId", tourDateId);
            sCmd.Parameters.AddWithValue("@TicketType", TicketType);
            sCmd.Parameters.AddWithValue("@CouponCode", couponCode);
            DataSet ds = sqlManager.ExecuteDataSet("GetTicketCost", SqlCommandType.StoredProcedure, sCmd);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (!String.IsNullOrEmpty(ds.Tables[0].Rows[0][0].ToString()))
                {
                    cost = Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
                }
                else
                {
                    cost = 0;
                }
            }
            else
            {
                cost = 0;
            }
            return cost;
        }

        public DataSet GetCouponsList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("SpGetCouponsListNew", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetCouponsListByEventId(int EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", EventID);
            var ds = sqlManager.ExecuteDataSet("SpGetCouponsListByEventID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet GetTempEventByEventId(int EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", EventID);
            var ds = sqlManager.ExecuteDataSet("SpGetTempEventByEventId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetCouponsListByUserId(int UserID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserID);
            var ds = sqlManager.ExecuteDataSet("SpGetCouponsListByUserID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetCouponsListNew(string sEventName, string packageName, string sCouponCode)
        {
            string qry = string.Empty;
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@sEventName", sEventName);
            sCmd.Parameters.AddWithValue("@packageName", packageName);
            sCmd.Parameters.AddWithValue("@sCouponCode", sCouponCode);
            DataSet ds = sqlManager.ExecuteDataSet("SpGetCouponsListNew", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public bool SpAddCouponsNew(string EventName, string CouponCode, decimal Discount, DateTime ExpiryDate, int CreatedBy, string TicketCategory)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpAddCoupons";
                sCmd.Parameters.AddWithValue("@EventName", EventName);
                sCmd.Parameters.AddWithValue("@CouponCode", CouponCode);
                sCmd.Parameters.AddWithValue("@Discount", Discount);
                sCmd.Parameters.AddWithValue("@ExpiryDate", ExpiryDate);
                sCmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                sCmd.Parameters.AddWithValue("@TicketCategory", TicketCategory);
                cn.Open();
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        public bool SpUpdateCouponsNew(int _CouponId, string EventName, string CouponCode, decimal Discount, DateTime ExpiryDate, int CreatedBy, string TicketCategory)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpUpdateCoupons";
                sCmd.Parameters.AddWithValue("@EventName", EventName);
                sCmd.Parameters.AddWithValue("@CouponCode", CouponCode);
                sCmd.Parameters.AddWithValue("@Discount", Discount);
                sCmd.Parameters.AddWithValue("@ExpiryDate", ExpiryDate);
                sCmd.Parameters.AddWithValue("@Id", _CouponId);
                sCmd.Parameters.AddWithValue("@TicketCategory", TicketCategory);
                cn.Open();
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        public DataSet GetTicketingEventsList()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("spGetTicketingEventsList", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public bool CheckCouponCode(string EventName, string CouponId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@eventName", EventName);
            sCmd.Parameters.AddWithValue("@couponCode", CouponId);
            DataSet ds = sqlManager.ExecuteDataSet("spCheckCouponCode", SqlCommandType.StoredProcedure, sCmd);

            if (ds.Tables[0].Rows.Count > 0)
                return true;
            else
                return false;
        }

        public DataSet GetCouponsListByCouponId(int CouponId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@Id", CouponId);
            var ds = sqlManager.ExecuteDataSet("SpGetCouponsListByCouponID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public bool CheckCouponCodeForEdit(int CouponId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@couponId", CouponId);
            DataSet ds = sqlManager.ExecuteDataSet("spCheckCouponCodeForUpdate", SqlCommandType.StoredProcedure, sCmd);

            if (ds.Tables[0].Rows.Count > 0)
                return true;
            else
                return false;
        }

        public DataSet GetTicketCategoryListByEventId(int eventId)
        {
            string qry = string.Empty;
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", eventId);
            DataSet ds = sqlManager.ExecuteDataSet("SpGetTicketCategoryListByEventId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public bool SpDeleteCoupon(int id)
        {
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MusikaEntitiesADO"].ConnectionString);
            SqlCommand sCmd = new SqlCommand();
            try
            {
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Connection = cn;
                sCmd.CommandText = "SpDeleteCoupons";
                sCmd.Parameters.AddWithValue("@Id", id);

                cn.Open();
                sCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                sCmd.Dispose();
                cn.Dispose();
            }
        }

        //GetCouponsId
        public DataSet GetCouponsId(int ID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@couponId", ID);
            var ds = sqlManager.ExecuteDataSet("SpGetCouponsByID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetUserEventsSearch(string searchTerm, int UserID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@searchTerm", searchTerm);
            sCmd.Parameters.AddWithValue("@UserID", UserID);
            var ds = sqlManager.ExecuteDataSet("GetEventListByArtistTickets", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetTicketsEventSummaryList(int UserID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserID", UserID);
            var ds = sqlManager.ExecuteDataSet("GetTicketsSummary", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetUserProfileByUserId(Int64 UserID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserID", UserID);
            var ds = sqlManager.ExecuteDataSet("Sp_GetUserProfileByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public int GetValidateTicketByUserEquals(int? ticketEventId, int? UserId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", ticketEventId);
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            var ds = sqlManager.ExecuteDataSet("Sp_ValidateTicketByUserEquals", SqlCommandType.StoredProcedure, sCmd);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString()) > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        public DataSet GetmatchedBarCode(string TicketNumber)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TicketNumber", TicketNumber);
            var ds = sqlManager.ExecuteDataSet("GetmatchedBarCode", SqlCommandType.StoredProcedure, sCmd);
            //var ds = sqlManager.ExecuteDataSet("GetTicketBuyUser", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }



        public string GetValidateTicketByUser(Guid? TicketNumber, int? UserId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TicketNumber", TicketNumber);
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            var ds = sqlManager.ExecuteDataSet("Sp_ValidateTicketByUserNew", SqlCommandType.StoredProcedure, sCmd);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                return "";
            }
        }

        public string GetValidateBulkTicketByUser(string TicketNumber, int? UserId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TicketNumber", TicketNumber);
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            var ds = sqlManager.ExecuteDataSet("Sp_ValidateBulkTicketByUser", SqlCommandType.StoredProcedure, sCmd);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                return "";
            }
        }

        //sp_GetTicketBalanceSummary
        public DataSet GetTicketBalanceSummary(int eventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", eventId);
            var ds = sqlManager.ExecuteDataSet("sp_GetTicketBalanceSummary", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }


        //sp_GetTicketBalanceSummary
        public DataSet GetTicketBuyUser(int eventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", eventId);
            var ds = sqlManager.ExecuteDataSet("GetTicketBuyUser", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetMatchedTicketNumber(string TicketNumber)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TicketNumber", TicketNumber);

            var ds = sqlManager.ExecuteDataSet("getticketnumber", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet GetMatchedTicketSerailNumber(string TicketSerialNumber)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TicketSerialNumber", TicketSerialNumber);

            var ds = sqlManager.ExecuteDataSet("getticketnumberSerailnumber", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        //Sp_TicketingEventsNew
        public DataSet GetTicketingEventsNewByEventID(int eventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", eventId);
            var ds = sqlManager.ExecuteDataSet("Sp_TicketingEventsNew", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        public DataSet GetTicketingEventsDetailAdmin(int eventId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", eventId);
            var ds = sqlManager.ExecuteDataSet("Sp_TicketingEventsDetailAdmin", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        //sp_GetUsersGoingForEventList
        public DataSet GetUsersGoingForEventList(int tourDate, int userId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@TourDate", tourDate);
            sCmd.Parameters.AddWithValue("@UserID", userId);
            var ds = sqlManager.ExecuteDataSet("sp_GetUsersGoingForEventList", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        //sp_ViewYourPlansDetailByUserId
        public DataSet GetViewYourPlansDetailByUserId(int userId)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserID", userId);
            var ds = sqlManager.ExecuteDataSet("sp_ViewYourPlansDetailByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        #region "Get User Details by UserID"
        public DataSet GetUserDetailsByUserId(int uid)
        {
            //spGetUserDetails
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserID", uid);
            var ds = sqlManager.ExecuteDataSet("spGetUserDetails", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        #endregion

        #region "Get Events List by UserID"
        //SP_GetEventsListByUserId
        public DataSet GetEventsListByUserId(int uid)
        {
            //spGetUserDetails
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserID", uid);
            var ds = sqlManager.ExecuteDataSet("SP_GetEventsListByUserId", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        #endregion

        #region "Update User Activation Status Based on Activation Code"
        //sp_UpdateUserActivationStatus
        public DataSet UpdateUserActivationStatus(string ActivationCode)
        {
            int uid;
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@ActivationCode", ActivationCode);
            var ds = sqlManager.ExecuteDataSet("sp_UpdateUserActivationStatus", SqlCommandType.StoredProcedure, sCmd);

            if (!String.IsNullOrEmpty(ds.Tables[0].Rows[0][0].ToString()))
            {
                uid = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

                var ds1 = this.GetUserDetailsByUserId(Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString()));
                return ds1;
            }
            else
            {
                return null;
            }
        }
        #endregion


        //sp_GetTicketsDetaislByTourDateID
        public DataSet GetTicketsDetaislByTourDateID(int tourDateId, int userid)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@tourDateID", tourDateId);
            sCmd.Parameters.AddWithValue("@userid", userid);
            var ds = sqlManager.ExecuteDataSet("sp_GetTicketsDetaislByTourDateID", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        //sp_GetTicketsDetaislByTourDateID
        public DataSet GetUsersTicketStatus(int UserId, int EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            sCmd.Parameters.AddWithValue("@EventId", EventID);
            var ds = sqlManager.ExecuteDataSet("SpGetUsersTicketStatus", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

        //sp_GetTicketsDetaislByTourDateID
        public DataSet GetCSVUserStatus(string EventID)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@EventId", EventID);
            var ds = sqlManager.ExecuteDataSet("GetCSVUserStatus", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        //sp_GetArtist
        public DataSet GetArtist()
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            var ds = sqlManager.ExecuteDataSet("sp_GetArtistData", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }
        public DataSet SpSearchAttendesTicketStatus(int UserId, int EventID, string searchBy)
        {
            var sqlManager = new SqlManager();
            var sCmd = new SqlCommand();
            sCmd.Parameters.AddWithValue("@UserId", UserId);
            sCmd.Parameters.AddWithValue("@EventId", EventID);
            //if (!String.IsNullOrEmpty(searchBy))
            //{
            sCmd.Parameters.AddWithValue("@SearchBy", searchBy);
            //}

            var ds = sqlManager.ExecuteDataSet("SpSearchAttendesTicketStatus", SqlCommandType.StoredProcedure, sCmd);
            return ds;
        }

    }
}
