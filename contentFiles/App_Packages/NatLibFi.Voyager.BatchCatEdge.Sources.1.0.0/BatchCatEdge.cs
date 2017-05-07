//
// Copyright 2017 University Of Helsinki (The National Library Of Finland)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

namespace NatLibFi.Voyager {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using BatchCat;
  
  class ConnectException : System.Exception {

    private int ErrorCodeInternal;
    public int ErrorCode {
      get { return this.ErrorCodeInternal; }
    }
    
    public ConnectException(string message, int error_code) : base(message) {
      this.ErrorCodeInternal = error_code;
    }
  }

  class Session {

    public ClassBatchCat bc;
    public DateTime time;
    
    public Session(string iniDir, string username, string password) {

      ConnectReturnCodes retval;

      this.bc = new ClassBatchCat();
      this.bc.Connect(ref iniDir, ref username, ref password, out retval);
      
      if ((int)retval == 0) {
        this.time = DateTime.Now;
      } else {
        throw new ConnectException(retval.ToString(), (int)retval);
      }

    }
    
  }
  
  public class BatchCatEdge {

    // 60 minutes
    public const int defaultSessionTimeoutMillis = 3600000;
    private int sessionTimeoutMillis = defaultSessionTimeoutMillis;
    private static Dictionary<string, Session> sessions = new Dictionary<string, Session>();

    private void RemoveExpiredSessions() {

      List<string> expiredSessions = new List<string>();
    
      foreach (KeyValuePair<string, Session> pair in BatchCatEdge.sessions) {
        if (DateTime.Now.Subtract(pair.Value.time).TotalMilliseconds > this.sessionTimeoutMillis) {
          expiredSessions.Add(pair.Key);
        }
      }
      
      expiredSessions.ForEach(item => { BatchCatEdge.sessions.Remove(item); });
      
    }

    private Session GetSession(string iniDir, string username, string password) {

      Session session;
      string key = iniDir + ":" + username;
      
      this.RemoveExpiredSessions();

      if (BatchCatEdge.sessions.ContainsKey(key)) {
        session = BatchCatEdge.sessions[key];
        session.time = DateTime.Now;
      } else {
        session = new Session(iniDir, username, password);
        BatchCatEdge.sessions.Add(key, session);
      }

      return session;

      
    }
    
    #pragma warning disable CS1998
      public async Task<object> SetSessionOptions(dynamic input) {
      this.sessionTimeoutMillis = input.sessionTimeout;
      return null;
    }
    
    #pragma warning disable CS1998
      public async Task<object> AddBibRecord(dynamic input) {
    
      AddBibReturnCode retval;
      Session session;
      int recordId;
      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      int library = input.library;
      int catLocation = input.catLocation;
      bool opacSuppress = input.opacSuppress;
      string okToExport = null;

      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {    
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;
      }

      session.bc.AddBibRecord(ref recordData, ref library, ref catLocation, ref opacSuppress, ref okToExport, out retval); 
      
      if (retval == 0) {
        session.bc.get_RecordIDAdded(out recordId);
        return new { recordId = recordId.ToString() };
      } else {
        return new { error = new { code = (int)retval, message = retval } };
      }
     
    }

    #pragma warning disable CS1998
      public async Task<object> UpdateBibRecord(dynamic input) {
    
      UpdateBibReturnCode retval;
      Session session; 
      int recordId = input.recordId;
      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      DateTime updateDate = DateTime.Parse(input.updateDate);
      int library = input.library;
      int catLocation = input.catLocation;
      bool opacSuppress = input.opacSuppress;
      string okToExport = null;
      string exportWithNewDate = null;
      
      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } };
      }

      session.bc.UpdateBibRecord(ref recordId, ref recordData, ref updateDate, ref library, ref catLocation, ref opacSuppress, ref okToExport, ref exportWithNewDate, out retval);      
      return retval == 0 ? null : new { error = new { code = (int)retval, message = retval } };
      
    }
    
    #pragma warning disable CS1998
      public async Task<object> DeleteBibRecord(dynamic input) {
    
      DeleteBibReturnCode retval;
      Session session;
      int recordId = input.recordId;
    
      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;        
      }
    
      session.bc.DeleteBibRecord(ref recordId, out retval); 
      return retval == 0 ? null : new { error = new {code = (int)retval, message = retval } };
    
    }




    #pragma warning disable CS1998
    public async Task<object> AddAuthorityRecord(dynamic input) {
    
      AddAuthorityReturnCode retval;
      Session session;
      int recordId;
      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      int catLocation = input.catLocation;
      string okToExport = null;

      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {    
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;
      }

      session.bc.AddAuthorityRecord(ref recordData, ref catLocation, ref okToExport, out retval); 
      
      if (retval == 0) {
        session.bc.get_RecordIDAdded(out recordId);
        return new { recordId = recordId.ToString() };
      } else {
        return new { error = new { code = (int)retval, message = retval } };
      }
     
    }

    #pragma warning disable CS1998
    public async Task<object> UpdateAuthorityRecord(dynamic input) {
    
      UpdateAuthorityReturnCode retval;
      Session session; 
      int recordId = input.recordId;
      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      DateTime updateDate = DateTime.Parse(input.updateDate);
      int catLocation = input.catLocation;
      string okToExport = null;
      string exportWithNewDate = null;
      
      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } };
      }

      session.bc.UpdateAuthorityRecord(ref recordId, ref recordData, ref updateDate, ref catLocation, ref okToExport, ref exportWithNewDate, out retval);      
      return retval == 0 ? null : new { error = new { code = (int)retval, message = retval } };
      
    }

    #pragma warning disable CS1998
    public async Task<object> DeleteAuthorityRecord(dynamic input) {
      
      DeleteAuthorityReturnCode retval;
      Session session;
      int recordId = input.recordId;
    
      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;        
      }
    
      session.bc.DeleteAuthorityRecord(ref recordId, out retval); 
      return retval == 0 ? null : new { error = new {code = (int)retval, message = retval } };
    
    }
    



    #pragma warning disable CS1998
      public async Task<object> AddHoldingRecord(dynamic input) {
    
      AddHoldingReturnCode retval;
      Session session;
      int recordId;

      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      int relatedRecordId = input.relatedRecordId;
      int catLocation = input.catLocation;
      bool opacSuppress = input.opacSuppress;
      int holdLocation = input.holdLocation;
      string okToExport = null;

      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {    
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;
      }

      session.bc.AddHoldingRecord(ref recordData, ref relatedRecordId, ref catLocation, ref opacSuppress, ref holdLocation, ref okToExport, out retval); 
      
      if (retval == 0) {
        session.bc.get_RecordIDAdded(out recordId);
        return new { recordId = recordId.ToString() };
      } else {
        return new { error = new { code = (int)retval, message = retval } };
      }
     
    }

    #pragma warning disable CS1998
      public async Task<object> UpdateHoldingRecord(dynamic input) {
    
      UpdateHoldingReturnCode retval;
      Session session; 
      int recordId = input.recordId;
      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      DateTime updateDate = DateTime.Parse(input.updateDate);
      int catLocation = input.catLocation;
      int relatedRecordId = input.relatedRecordId;
      int holdLocation = input.holdLocation;
      bool opacSuppress = input.opacSuppress;
      string okToExport = null;
      string exportWithNewDate = null;
      
      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } };
      }

      session.bc.UpdateHoldingRecord(ref recordId, ref recordData, ref updateDate, ref catLocation, ref relatedRecordId, ref holdLocation, ref opacSuppress, ref okToExport, ref exportWithNewDate, out retval);      
      return retval == 0 ? null : new { error = new { code = (int)retval, message = retval } };
      
    }
    
    #pragma warning disable CS1998
      public async Task<object> DeleteHoldingRecord(dynamic input) {
    
      DeleteHoldingReturnCode retval;
      Session session;
      int recordId = input.recordId;
    
      try {
        session = this.GetSession(input.iniDir, input.username, input.password);
      } catch (ConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;        
      }
    
      session.bc.DeleteHoldingRecord(ref recordId, out retval); 
      return retval == 0 ? null : new { error = new {code = (int)retval, message = retval } };
    
    }

  }
  
}
