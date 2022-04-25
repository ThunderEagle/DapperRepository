using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperRepository;

namespace Example {
  class ConnectionFactory :IConnectionFactory {
    public IDbConnection GetConnection() {
      var cn = new SqlConnection("server=localhost;database=VendorPay;Integrated Security=True");
      cn.Open();
      return cn;
    }
  }
}
