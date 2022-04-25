using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperRepository;
using DapperRepository.Exceptions;
using Example.Models;

namespace Example.Repositories {
  public class VendorRepository {

    public VendorRepository(IConnectionFactory connectionFactory) {
      ConnectionFactory = connectionFactory.GetConnection;
    }

    protected Func<IDbConnection> ConnectionFactory { get; }

    public IList<Vendor> GetAll() {

      try {
        var sql = "SELECT VENDORS.*, ADDRESSES.* FROM VENDORS LEFT JOIN ADDRESSES ON VENDORS.AddressId = ADDRESSES.Id";

        using(var cn = ConnectionFactory.Invoke()) {
          var result = cn.Query<Vendor, Address, Vendor>(sql, (v, a) => {
                                                                v.Address = a;
                                                                return v;
                                                              });

          return result != null ? result.ToList() : new List<Vendor>();
        }
      }
      catch(RepositoryException) {
        throw;
      }
      catch(Exception e) {
        throw new RepositoryException(MethodBase.GetCurrentMethod(), e);
      }

    }

  }
}
