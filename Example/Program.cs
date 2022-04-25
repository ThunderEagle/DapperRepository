using System;
using ConsoleDump;
using Example.Models;
using DapperRepository;
using DapperRepository.DataMapping;
using Example.Repositories;

namespace Example // Note: actual namespace depends on the project name.
{
  internal class Program {
    static void Main(string[] args) {

      //BaseRepo();

      ManualRepo();


    }


    static void ManualRepo() {
      var connectionFactory = new ConnectionFactory();
      var vendorRepo = new VendorRepository(connectionFactory);

      var vendors = vendorRepo.GetAll();

      foreach (var vendor in vendors) {
        vendor.Dump();
        vendor.Address.Dump();
      }

    }


    static void BaseRepo() {
      var connectionFactory = new ConnectionFactory();
      var vendorRepo = new ReadOnlyRepository<Vendor>(connectionFactory, new SQLBuilder<Vendor>(new EntityMapParser()));
      var addressRepo = new ReadOnlyRepository<Address>(connectionFactory, new SQLBuilder<Address>(new EntityMapParser()));


      var vendors = vendorRepo.GetAll();

      foreach (var vendor in vendors) {
        if (vendor.AddressID != null) {
          vendor.Address = addressRepo.Get(vendor.AddressID.Value);
        }
        vendor.Dump();
        vendor.Address.Dump();
      }

    }

  }
}