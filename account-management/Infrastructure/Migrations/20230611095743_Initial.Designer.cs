﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlatformPlatform.AccountManagement.Infrastructure;

#nullable disable

namespace PlatformPlatform.AccountManagement.Infrastructure.Migrations
{
    [DbContext(typeof(AccountManagementDbContext))]
    [Migration("20230611095743_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PlatformPlatform.AccountManagement.Domain.Tenants.Tenant", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(30)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ModifiedAt")
                        .IsConcurrencyToken()
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Phone")
                        .HasColumnType("varchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("PlatformPlatform.AccountManagement.Domain.Users.User", b =>
                {
                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("varchar(30)");

                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ModifiedAt")
                        .IsConcurrencyToken()
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("UserRole")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PlatformPlatform.AccountManagement.Domain.Users.User", b =>
                {
                    b.HasOne("PlatformPlatform.AccountManagement.Domain.Tenants.Tenant", null)
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
