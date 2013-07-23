﻿/*                       ____               ____________
 *                      |    |             |            |
 *                      |    |             |    ________|
 *                      |    |             |   |
 *                      |    |             |   |    
 *                      |    |             |   |    ____
 *                      |    |             |   |   |    |
 *                      |    |_______      |   |___|    |
 *                      |            |  _  |            |
 *                      |____________| |_| |____________|
 *                        
 *      Author(s):      limpygnome (Marcus Craske)              limpygnome@gmail.com
 * 
 *      License:        Creative Commons Attribution-ShareAlike 3.0 Unported
 *                      http://creativecommons.org/licenses/by-sa/3.0/
 * 
 *      File:           User.cs
 *      Path:           /App_Code/CMS/Plugins/Basic Site Auth/User.cs
 * 
 *      Change-Log:
 *                      2013-07-06      Created initial class.
 * 
 * *********************************************************************************************************************
 * A model to represent a user of the basic site authentication plugin. Modifying the settings of a user can be dirty,
 * with the data validated by the save method.
 * *********************************************************************************************************************
 */
using System;
using CMS.BasicSiteAuth;
using CMS.Base;
using UberLib.Connector;

namespace CMS
{
    namespace BasicSiteAuth
    {
        /// <summary>
        /// A model to represent a user of the basic site authentication plugin. Modifying the settings of a user
        /// can be dirty, with the data validated by the save method.
        /// </summary>
        public class User
        {
            // Enums ***************************************************************************************************
            /// <summary>
            /// The status of saving a user.
            /// </summary>
            public enum UserCreateSaveStatus
            {
                Success = 0,
                InvalidUsername_Length = 100,
                InvalidUsername_AlreadyExists = 101,
                InvalidPassword_Length = 200,
                InvalidEmail_Length = 300,
                InvalidEmail_Format = 301,
                InvalidSecretQuestion_Length = 400,
                InvalidSecretAnswer_Length = 500,
                InvalidUserGroup = 600,
                Error_Regisration = 900
            };
            // Fields **************************************************************************************************
            int         userID;             // The identifier of the user.
            string      username,           // The username of the user.
                        password,           // The user'ss password (hashed).
                        email,              // The user's e-mail.
                        secretQuestion,     // Account recovery: secret question.
                        secretAnswer;       // Account recovery: secret answer.
            UserGroup   userGroup;          // The user's role/group.
            DateTime    registered;         // The date and time of when the user registered.
            bool        loaded,             // Indicates if this instance has been loaded (true) or is a new user and does not exist (false).
                        modified;           // Indicates if the data has been modified.
            // Methods - Constructors **********************************************************************************
            public User()
            {
                modified = loaded = false;
            }
            // Mehods - Database ***************************************************************************************
            /// <summary>
            /// Loads a user from the database.
            /// </summary>
            /// <param name="bsa">BSA plugin.</param>
            /// <param name="conn">Database connector.</param>
            /// <param name="userID">The identifier of the user.</param>
            /// <returns>The user if found/valid, else null.</returns>
            public static User load(BasicSiteAuth bsa, Connector conn, int userID)
            {
                Result result = conn.queryRead("SELECT * FROM bsa_users WHERE userid='" + SQLUtils.escape(userID.ToString()) + "';");
                if (result.Count == 1)
                    return load(bsa, result[0]);
                else
                    return null;
            }
            /// <summary>
            /// Loads a user from database data.
            /// </summary>
            /// <param name="bsa">BSA plugin.</param>
            /// <param name="data">Database data.</param>
            /// <returns>The user if found/valid, else null.</returns>
            public static User load(BasicSiteAuth bsa, ResultRow data)
            {
                UserGroup ug = bsa.UserGroups[int.Parse(data["groupid"])];
                if (ug == null)
                    return null;
                User usr = new User();
                usr.loaded = true;
                usr.userID = int.Parse(data["userid"]);
                usr.username = data["username"];
                usr.password = data["password"];
                usr.email = data["email"];
                usr.secretQuestion = data["secret_question"];
                usr.secretAnswer = data["secret_answer"];
                usr.userGroup = ug;
                usr.registered = data.get2<DateTime>("datetime_register");
                return usr;
            }
            /// <summary>
            /// Persists the user's data to the database.
            /// </summary>
            /// <param name="conn">Database connector.</param>
            /// <returns>The state from attempting to persist the user.</returns>
            public UserCreateSaveStatus save(BasicSiteAuth bsa, Connector conn)
            {
                lock(this)
                {
                    // Validate the data
                    if (username.Length < Core.Settings[BasicSiteAuth.SETTINGS_USERNAME_MIN].get<int>() || username.Length > Core.Settings[BasicSiteAuth.SETTINGS_USERNAME_MAX].get<int>())
                        return UserCreateSaveStatus.InvalidUsername_Length;
                    else if (password.Length < Core.Settings[BasicSiteAuth.SETTINGS_PASSWORD_MIN].get<int>() || password.Length > Core.Settings[BasicSiteAuth.SETTINGS_PASSWORD_MAX].get<int>())
                        return UserCreateSaveStatus.InvalidPassword_Length;
                    else if (email.Length < Core.Settings[BasicSiteAuth.SETTINGS_EMAIL_MIN].get<int>() || email.Length > Core.Settings[BasicSiteAuth.SETTINGS_EMAIL_MAX].get<int>())
                        return UserCreateSaveStatus.InvalidEmail_Length;
                    else if (secretQuestion.Length < Core.Settings[BasicSiteAuth.SETTINGS_SECRETQUESTION_MIN].get<int>() || secretQuestion.Length > Core.Settings[BasicSiteAuth.SETTINGS_SECRETQUESTION_MAX].get<int>())
                        return UserCreateSaveStatus.InvalidSecretQuestion_Length;
                    else if (secretAnswer.Length < Core.Settings[BasicSiteAuth.SETTINGS_SECRETANSWER_MIN].get<int>() || secretAnswer.Length > Core.Settings[BasicSiteAuth.SETTINGS_SECRETANSWER_MAX].get<int>())
                        return UserCreateSaveStatus.InvalidSecretAnswer_Length;
                    else if (userGroup == null || !bsa.UserGroups.contains(userGroup))
                        return UserCreateSaveStatus.InvalidUserGroup;
                    else if (!BSAUtils.validEmail(email))
                        return UserCreateSaveStatus.InvalidEmail_Format;
                    // Persist the data
                    SQLCompiler sql = new SQLCompiler();
                    sql["username"] = username;
                    sql["password"] = password;
                    sql["email"] = email;
                    sql["secret_question"] = secretQuestion;
                    sql["secret_answer"] = secretAnswer;
                    sql["groupid"] = userGroup.GroupID.ToString();
                    sql["datetime_register"] = registered.ToString("YYYY-MM-dd HH:mm:ss");
                    if(loaded)
                        conn.queryExecute(sql.compileUpdate("bsa_users", "userid='" + SQLUtils.escape(userID.ToString()) + "'"));
                    else
                        userID = int.Parse(conn.queryScalar(sql.compileInsert("bsa_users", "userid")).ToString());
                    // Success! Reset flags and return status
                    modified = false;
                    return UserCreateSaveStatus.Success;
                }
            }
            // Methods - Properties ************************************************************************************
            /// <summary>
            /// The user's identifier.
            /// </summary>
            public int UserID
            {
                get
                {
                    return userID;
                }
            }
            /// <summary>
            /// The user's username.
            /// </summary>
            public string Username
            {
                get
                {
                    return username;
                }
                set
                {
                    modified = true;
                    username = value;
                }
            }
            /// <summary>
            /// The user's password, hashed.
            /// </summary>
            public string Password
            {
                get
                {
                    return password;
                }
                set
                {
                    modified = true;
                    password = value;
                }
            }
            /// <summary>
            /// The user's e-mail address.
            /// </summary>
            public string Email
            {
                get
                {
                    return email;
                }
                set
                {
                    modified = true;
                    email = value;
                }
            }
            /// <summary>
            /// The user's secret-question; may be empty.
            /// </summary>
            public string SecretQuestion
            {
                get
                {
                    return secretQuestion;
                }
                set
                {
                    modified = true;
                    secretQuestion = value;
                }
            }
            /// <summary>
            /// The user's secret-answer; may be empty.
            /// </summary>
            public string SecretAnswer
            {
                get
                {
                    return secretAnswer;
                }
                set
                {
                    modified = true;
                    secretAnswer = value;
                }
            }
            /// <summary>
            /// The user's role/group.
            /// </summary>
            public UserGroup UserGroup
            {
                get
                {
                    return userGroup;
                }
                set
                {
                    modified = true;
                    userGroup = value;
                }
            }
            /// <summary>
            /// Indicates if the data has been modified.
            /// </summary>
            public bool IsModified
            {
                get
                {
                    return modified;
                }
            }
        }
    }
}