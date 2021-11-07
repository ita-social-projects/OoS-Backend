﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OutOfSchool.IdentityServer.KeyManagement;

namespace OutOfSchool.IdentityServer.Data.Migrations.IdentityServer.CertificateDb
{
    [DbContext(typeof(CertificateDbContext))]
    [Migration("20211107073924_InitialIdentityServerCertificateDbMigration")]
    partial class InitialIdentityServerCertificateDbMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("OutOfSchool.IdentityServer.KeyManagement.SigningCertificate", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("CertificateBase64")
                        .HasColumnType("longtext");

                    b.Property<DateTimeOffset>("ExpirationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.HasKey("Id");

                    b.ToTable("Certificates");
                });
#pragma warning restore 612, 618
        }
    }
}
