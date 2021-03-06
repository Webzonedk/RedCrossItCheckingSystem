﻿using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using RedCrossItCheckingSystem.Data;
using RedCrossItCheckingSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RedCrossItCheckingSystem.DAL
{
    public class DbManager
    {
        //private fields containting connectionstrings for databases
        private readonly IConfiguration configuration;
        private readonly string itemsConnectionString;
        private readonly string usersConnectionString;
       
        //constructor setting connectionstrings to databases
        public DbManager(IConfiguration _configuration)
        {
            this.configuration = _configuration;
            itemsConnectionString = configuration.GetConnectionString("ItemDBContext");
            usersConnectionString = configuration.GetConnectionString("UserDBContext");
        }


        //method to get data from database, based on serial number
        internal List<DataContainer> GetDataFromserial(DataContainer container)
        {
            SqlConnection con = new SqlConnection(itemsConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("GetData", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@SerialNumber", System.Data.SqlDbType.VarChar).Value = container.SerialNumber;

            List<DataContainer> containers = new List<DataContainer>();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                int counter = 0;

                //read the data and count for every row
                while (reader.Read())
                {
                    DataContainer output = new DataContainer();

                    output.SerialNumber = (string)reader["serialNumber"];
                    output.CaseID = (int)reader["caseID"];
                    output.Accessories = (string)reader["accessories"];
                    output.DeviceName = (string)reader["deviceName"];
                    output.DeviceType = (string)reader["deviceType"];
                    output.Status = (string)reader["status"];

                    counter++;


                    // check if reader has rows
                    if (counter > 0)
                    {
                        output.IsValid = true;
                        containers.Add(output);

                    }
                }

            }
            catch (Exception)
            {
                DataContainer output = new DataContainer();
               
                //disable container if no connection to database
                output.IsValid = false;
            }


            con.Close();
            return containers;
        }

        //method for getting data out of database, based on case id
        internal DataContainer GetDataFromCaseID(int caseID)
        {
            SqlConnection con = new SqlConnection(itemsConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("GetData_caseID", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@caseID", System.Data.SqlDbType.Int).Value = caseID;

            DataContainer container = new DataContainer();
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();

               
                //read the data
                while (reader.Read())
                {
                    DataLog log = new DataLog();

                    container.SerialNumber = (string)reader["serialNumber"];
                    container.CaseID = (int)reader["caseID"];
                    container.Accessories = (string)reader["accessories"];
                    container.DeviceName = (string)reader["deviceName"];
                    container.DeviceType = (string)reader["deviceType"];
                    container.Status = (string)reader["status"];

                    container.IsValid = true;
                    log.Description = (string)reader["description"];
                    log.Department = (string)reader["department"];
                    log.EmplyeeName = (string)reader["employeeName"];
                    log.LogDate = reader.GetDateTime(reader.GetOrdinal("logdate"));

                    container.DataLogs.Add(log);
                }

            }
            catch (Exception)
            {
                //disable container if no connection to database
                container.IsValid = false;
            }


            con.Close();
            return container;

        }

        //method to send log data to database
        internal void SetData(DataContainer container)
        {
            SqlConnection con = new SqlConnection(itemsConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("SetData", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            int lastLog = container.DataLogs.Count - 1;


            cmd.Parameters.Add("@caseID", System.Data.SqlDbType.VarChar).Value = container.CaseID;
            cmd.Parameters.Add("@SerialNumber", System.Data.SqlDbType.VarChar).Value = container.SerialNumber;
            cmd.Parameters.Add("@department", System.Data.SqlDbType.VarChar).Value = container.DataLogs[lastLog].Department;
            cmd.Parameters.Add("@logDate", System.Data.SqlDbType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@status", System.Data.SqlDbType.VarChar).Value = container.Status;
            cmd.Parameters.Add("@deviceName", System.Data.SqlDbType.VarChar).Value = container.DeviceName;
            cmd.Parameters.Add("@deviceType", System.Data.SqlDbType.VarChar).Value = container.DeviceType;
            cmd.Parameters.Add("@accessories", System.Data.SqlDbType.VarChar).Value = container.Accessories;
            cmd.Parameters.Add("@employeeName", System.Data.SqlDbType.VarChar).Value = container.DataLogs[lastLog].EmplyeeName;
            cmd.Parameters.Add("@description", System.Data.SqlDbType.VarChar).Value = container.DataLogs[lastLog].Description;

            cmd.ExecuteNonQuery();
            con.Close();
        }

        //int method to add new case data to database. method returns the case id after inserting into database
        internal int CreateData(DataContainer container)
        {
            SqlConnection con = new SqlConnection(itemsConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("CreateData", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@SerialNumber", System.Data.SqlDbType.VarChar).Value = container.SerialNumber;
            cmd.Parameters.Add("@department", System.Data.SqlDbType.VarChar).Value = container.DataLogs[0].Department;
            cmd.Parameters.Add("@logDate", System.Data.SqlDbType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@status", System.Data.SqlDbType.VarChar).Value = container.Status;
            cmd.Parameters.Add("@deviceName", System.Data.SqlDbType.VarChar).Value = container.DeviceName;
            cmd.Parameters.Add("@deviceType", System.Data.SqlDbType.VarChar).Value = container.DeviceType;
            cmd.Parameters.Add("@accessories", System.Data.SqlDbType.VarChar).Value = container.Accessories;
            cmd.Parameters.Add("@employeeName", System.Data.SqlDbType.VarChar).Value = container.DataLogs[0].EmplyeeName;
            cmd.Parameters.Add("@description", System.Data.SqlDbType.VarChar).Value = container.DataLogs[0].Description;
            cmd.Parameters.Add("@caseId", System.Data.SqlDbType.Int).Direction = ParameterDirection.Output;
             cmd.ExecuteNonQuery();

           
            int output = int.Parse(cmd.Parameters["@caseId"].Value.ToString());


            con.Close();
            return output;
        }

        //bool method takes in userdata, SQL query checks if username & password exsist. Returns true or false
        internal bool UserLogin(UserData user)
        {
            SqlConnection con = new SqlConnection(usersConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("CheckUserLogin", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@userName", System.Data.SqlDbType.VarChar).Value = user.UserName;
            cmd.Parameters.Add("@password", System.Data.SqlDbType.VarChar).Value = user.Password;


            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Debug.Write("username exists");
                    con.Close();
                    return true;
                }
                else
                {
                    Debug.Write("no username or password");
                    con.Close();
                    return false;
                }

            }
            catch (Exception)
            {
                Debug.Write("error");
                return false;
            }
        }

        // method returns a list of all data from database
        internal List<DataContainer> GetAllData()
        {
            SqlConnection con = new SqlConnection(itemsConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("GetAllData", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            List<DataContainer> containers = new List<DataContainer>();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                int counter = 0;

                //read the data and count for every row
                while (reader.Read())
                {

                    DataContainer output = new DataContainer();
                    output.SerialNumber = (string)reader["serialNumber"];
                    output.CaseID = (int)reader["caseID"];
                    output.Accessories = (string)reader["accessories"];
                    output.DeviceName = (string)reader["deviceName"];
                    output.DeviceType = (string)reader["deviceType"];
                    output.Status = (string)reader["status"];
                    output.LogCount = (int)reader["counter"];



                    counter++;
                    // check if reader has rows
                    if (counter > 0)
                    {
                        output.IsValid = true;
                        containers.Add(output);
                    }
                }

            }
            catch (Exception)
            {
                DataContainer output = new DataContainer();
                //disable container if no connection
                output.IsValid = false;
            }


            con.Close();
            return containers;
        }
    }
}
