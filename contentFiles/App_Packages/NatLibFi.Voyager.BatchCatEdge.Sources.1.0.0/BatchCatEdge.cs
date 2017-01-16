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
  using System.Text;
  using System.Threading.Tasks;
  using BatchCat;

  class BatchCatConnectException : System.Exception {

    private int ErrorCodeInternal;
    public int ErrorCode {
      get { return this.ErrorCodeInternal; }
    }
    
    public BatchCatConnectException(string message, int error_code) : base(message) {
      this.ErrorCodeInternal = error_code;
    }
  }
  
  public class BatchCatEdge {

    private ClassBatchCat GetBatchCat(string ini_dir, string username, string password) {

      ClassBatchCat bc = new ClassBatchCat();
      ConnectReturnCodes retval;
      bc.Connect(ref ini_dir, ref username, ref password, out retval);

      if ((int)retval == 0) {
        return bc;
      } else {
        throw new BatchCatConnectException(retval.ToString(), (int)retval);
      }
      
    }

    #pragma warning disable CS1998
      public async Task<object> AddBibRecord(dynamic input) {
    
      AddBibReturnCode retval;
      ClassBatchCat bc;
      int recordId;
      string recordData = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(input.recordData));
      int library = input.library;
      int catLocation = input.catLocation;
      bool opacSuppress = input.opacSuppress;
      string okToExport = null;

      try {
        bc = this.GetBatchCat(input.iniDir, input.username, input.password);
      } catch (BatchCatConnectException e) {    
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;
      }

      bc.AddBibRecord(ref recordData, ref library, ref catLocation, ref opacSuppress, ref okToExport, out retval); 
      
      if (retval == 0) {
        bc.get_RecordIDAdded(out recordId);
        return new { recordId = recordId.ToString() };
      } else {
        return new { error = new { code = (int)retval, message = retval } };
      }
     
    }

    #pragma warning disable CS1998
      public async Task<object> UpdateBibRecord(dynamic input) {

      UpdateBibReturnCode retval;
      ClassBatchCat bc; 
      int recordId = input.recordId;
      string recordData = input.recordData;
      DateTime updateDate = DateTime.Parse(input.updateDate);
      int library = input.library;
      int catLocation = input.catLocation;
      bool opacSuppress = input.opacSuppress;
      string okToExport = null;
      string exportWithNewDate = null;
      
      try {
        bc = this.GetBatchCat(input.iniDir, input.username, input.password);
      } catch (BatchCatConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } };
      }

      bc.UpdateBibRecord(ref recordId, ref recordData, ref updateDate, ref library, ref catLocation, ref opacSuppress, ref okToExport, ref exportWithNewDate, out retval);      
      return retval == 0 ? null : new { error = new { code = (int)retval, message = retval } };
      
    }
    
    #pragma warning disable CS1998
      public async Task<object> DeleteBibRecord(dynamic input) {
    
      DeleteBibReturnCode retval;
      ClassBatchCat bc;
      int recordId = input.recordId;
    
      try {
        bc = this.GetBatchCat(input.iniDir, input.username, input.password);
      } catch (BatchCatConnectException e) {
        return new { error = new { message = e.Message, code = e.ErrorCode } } ;        
      }
    
      bc.DeleteBibRecord(ref recordId, out retval); 
      return retval == 0 ? null : new { error = new {code = (int)retval, message = retval } };
    
    }
    
  }
  
}
