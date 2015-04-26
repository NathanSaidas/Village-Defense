#region CHANGE LOG
// -- April     15, 2015 - Nathan Hanlan - Added AccountDetails class.
#endregion

using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;


namespace Gem
{
    //Provides information about an account.
    public class AccountDetails
    {
        private string m_Username = string.Empty;
        private string m_Password = string.Empty;

        public void Save(Stream aStream, IFormatter aFormatter)
        {
            aFormatter.Serialize(aStream, m_Username);
            aFormatter.Serialize(aStream, m_Password);
        }
        public bool Load(Stream aStream, IFormatter aFormatter)
        {
            try
            {
                m_Username = (string)aFormatter.Deserialize(aStream);
                m_Password = (string)aFormatter.Deserialize(aStream);
            }
            catch (Exception aException)
            {
                DebugUtils.LogException(aException);
                return false;
            }
            return true;
        }

        public string username
        {
            get { return m_Username; }
            set { m_Username = value; }
        }

        public string password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }
    }
}



