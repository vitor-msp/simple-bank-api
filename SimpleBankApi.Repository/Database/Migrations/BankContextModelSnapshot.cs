﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimpleBankApi.Repository.Database.Context;

#nullable disable

namespace simple_bank_api.Migrations
{
    [DbContext(typeof(BankContext))]
    partial class BankContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("SimpleBankApi.Repository.Database.Schema.AccountDB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountNumber")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Active")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("RefreshTokenExpiration")
                        .HasColumnType("TEXT");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AccountNumber")
                        .IsUnique();

                    b.HasIndex("OwnerId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("SimpleBankApi.Repository.Database.Schema.CustomerDB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Cpf")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Cpf")
                        .IsUnique();

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("SimpleBankApi.Repository.Database.Schema.TransactionDB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("OperatingAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RelatedAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RelatedTransactionId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TransactionType")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Value")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("OperatingAccountId");

                    b.HasIndex("RelatedAccountId");

                    b.HasIndex("RelatedTransactionId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("SimpleBankApi.Repository.Database.Schema.AccountDB", b =>
                {
                    b.HasOne("SimpleBankApi.Repository.Database.Schema.CustomerDB", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("SimpleBankApi.Repository.Database.Schema.TransactionDB", b =>
                {
                    b.HasOne("SimpleBankApi.Repository.Database.Schema.AccountDB", "OperatingAccount")
                        .WithMany()
                        .HasForeignKey("OperatingAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SimpleBankApi.Repository.Database.Schema.AccountDB", "RelatedAccount")
                        .WithMany()
                        .HasForeignKey("RelatedAccountId");

                    b.HasOne("SimpleBankApi.Repository.Database.Schema.TransactionDB", "RelatedTransaction")
                        .WithMany()
                        .HasForeignKey("RelatedTransactionId");

                    b.Navigation("OperatingAccount");

                    b.Navigation("RelatedAccount");

                    b.Navigation("RelatedTransaction");
                });
#pragma warning restore 612, 618
        }
    }
}
