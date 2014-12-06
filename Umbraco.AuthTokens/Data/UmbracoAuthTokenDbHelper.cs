﻿using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Umbraco.AuthTokens.Data
{
    public static class UserAuthTokenDbHelper
    {
        private static readonly UmbracoDatabase Database = ApplicationContext.Current.DatabaseContext.Database;

        /// <summary>
        /// Try & see if we can find a record in the DB based off the User ID
        /// </summary>
        /// <param name="userId">The user ID to try and find in the DB</param>
        /// <returns>Returns Auth Token record/object in DB or null if not found</returns>
        public static UmbracoAuthToken GetAuthToken(int userId)
        {
            //Try & find a record in the DB from the userId
            var findRecord = Database.SingleOrDefault<UmbracoAuthToken>("WHERE UserId=@0", userId);

            //Return the object (Will be null if can't find an item)
            return findRecord;
        }

        /// <summary>
        /// Insert a new Auth Token into the custom DB table OR
        /// Update just the auth token if we find a record for the backoffice user already
        /// </summary>
        /// <param name="authToken"></param>
        public static void InsertAuthToken(UmbracoAuthToken authToken)
        {
            //Just to be 100% sure for data sanity that a record for the user does not exist already
            var existingRecord = GetAuthToken(authToken.UserId);

            //Insert new record if no item exists already
            if (existingRecord == null)
            {
                //Getting issues with insert & ID being 0 causing a conflict
                Database.Insert(authToken);
            }
            else
            {
                //Update the existing record
                existingRecord.AuthToken = authToken.AuthToken;
                existingRecord.DateCreated = authToken.DateCreated;

                //Update these fields in DB just in case they have changed on the user
                existingRecord.UserName = authToken.UserName;
                existingRecord.UserType = authToken.UserType;

                //Save the existing record we found
                Database.Save(existingRecord);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        public static void DeleteAuthToken(int userId)
        {
            //Just to be 100% sure for data sanity that a record for the user does not exist already
            var existingRecord = GetAuthToken(userId);

            if (existingRecord != null)
            {
                //We found the record in the DB - let's remove/delete it
                Database.Delete<UmbracoAuthToken>("WHERE UserId=@0", userId);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static bool IsTokenValid(UmbracoAuthToken authToken)
        {
            //Let's verify the token we have
            //Is what we have in the DB matching on the UserID as lookup

            //Try & find record in DB on UserID
            var lookupRecord = GetAuthToken(authToken.UserId);

            //If we find a record in DB
            if (lookupRecord != null)
            {
                //Lets verify the token we have is the same on the DB record
                //May not match as the user may have saved the password & caused a new token to be generated
                return authToken.AuthToken == lookupRecord.AuthToken;
            }

            //No record found in the DB - so return false
            return false;
        }
    }
}
