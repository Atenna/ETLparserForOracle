﻿using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleConnectETLParser1.Objects;

namespace OracleConnectETLParser1.Controllers
{
    public class Manager
    {
        public List<DbObject> ListOfObjects;                   // MAIN list of DbObjects
        public List<DbObject> RelateDbObjects;                 // list of related objects - this list is filled by listOfRelatedObjects method
        /*
         * Creation of List<DB_object> - all DB objects transformed into Object: DB_Object
         * Objects are stored in List.
         */
        public void CreateDB_Objects()
        {
            DbConnector db = new DbConnector();
            db.Open();
            OracleCommand oraCmd = new OracleCommand();
            oraCmd.Connection = db.OraConnection;
            oraCmd.CommandText = "select object_name, object_type, owner from ALL_OBJECTS WHERE Owner ='"+db.DbOwner+"'";
            OracleDataReader dr = oraCmd.ExecuteReader();
            ListOfObjects = new List<DbObject>();
            while (dr.Read())
            {
                // name, type from all objects
                //ListOfObjects.Add(new DbObject(dr.GetString(0), dr.GetString(1)));
                string type = dr.GetString(1);
                switch (type)
                {
                    case ("VIEW"):
                        ListOfObjects.Add(new View(dr.GetString(0), dr.GetString(2)));
                        break;
                    case ("TABLE"):
                        ListOfObjects.Add(new Table(dr.GetString(0), dr.GetString(2)));
                        break;
                    case ("MATERIALIZED VIEW"):
                        ListOfObjects.Add(new View(dr.GetString(0), dr.GetString(2), true));
                        break;
                    case ("PROCEDURE"):
                        ListOfObjects.Add(new Procedure(dr.GetString(0), dr.GetString(2)));
                        break;
                    case ("FUNCTION"):
                        ListOfObjects.Add(new Function(dr.GetString(0), dr.GetString(2)));
                        break;
                    case ("TRIGGER"):
                        ListOfObjects.Add(new Trigger(dr.GetString(0), dr.GetString(2)));
                        break;
                    case ("SEQUENCE"):
                        ListOfObjects.Add(new Sequence(dr.GetString(0), dr.GetString(2)));
                        break;
                }

            }
            // some cycle will be necessary here - later
            SetNextLevel(2);
            SetNextLevel(3);
            // - END cycle
            SetListOfRelatedObjects();
            db.Close();
        }
        /*
         * Quite complicated method for DbObjects level setting.
         */
        private void SetNextLevel(int newLevel)                        
        {
            for (int i = 0; i < ListOfObjects.Count; i++)                                   // all objects
            {
                if (ListOfObjects[i].Level==-1)                                        // ==-1 (only unleveled DbObjects expected)
                {
                    for (int j = 0; j < ListOfObjects[i].ReferencedNames.Count; j++)       // reference objects for previous object
                    {
                    if (GetDbObjectLevel(ListOfObjects[i].ReferencedNames[j])<newLevel)    // checking if referenced object had less level than main object
                        {
                            ListOfObjects[i].Level = newLevel;
                        }
                    else
                        {
                            ListOfObjects[i].Level = -1;
                        }
                    }
                }
            }
        }

        /*
         * Get of DBObject level.
         * return: int - level
         * param: pname - DbObjectName
         */
        private int GetDbObjectLevel(string pname)                  // when I need return DB_object.level matched by pname
        {
           //DB_Object someoDbObject= new DB_Object();
            for (int i = 0; i < ListOfObjects.Count; i++)
            {
                if (ListOfObjects[i].Name==pname)
                {
                    return ListOfObjects[i].Level;
                }
            }
            return 999;
        }
        /*
         * Method return DBObject from List<> ListOfObjects based on pname - name ob object
         */
        private DbObject GetDbObjectFromListOfObjects(string pname)
        {
            for (int i = 0; i < ListOfObjects.Count; i++)
            {
                if (ListOfObjects[i].Name == pname)
                {
                    return ListOfObjects[i];
                }
            }
            return null;
        }

        /*
         * Method will set list of all related objects for selected Object in param
         * - this methot should rewrite atribute in object from DB_object class
        */

        private void SetListOfRelatedObjects()                           
        {
            // prejdem vsetky objekty v liste X - main list
            // pre vsetky objekty mena listu X.related urob
            // do X.ReferencedObjects=GetDbObjectFromListOfObjects(x.related[actual])
            for (int i = 0; i < ListOfObjects.Count; i++)
            {
                for (int j = 0; j < ListOfObjects[i].ReferencedNames.Count; j++)
                {
                    ListOfObjects[i].ReferencedObjects.Add(GetDbObjectFromListOfObjects(ListOfObjects[i].ReferencedNames[j]));
                }
            }
        }

    }
}
