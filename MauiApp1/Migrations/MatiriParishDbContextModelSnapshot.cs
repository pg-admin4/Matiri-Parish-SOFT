using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MauiApp1.Data;

#nullable disable

namespace MauiApp1.Migrations
{
    [DbContext(typeof(MatiriParishDbContext))]
    partial class MatiriParishDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("MauiApp1.Models.User", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("PasswordSalt")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("Role")
                    .HasColumnType("int");

                b.Property<bool>("IsMfaEnabled")
                    .HasColumnType("bit");

                b.Property<string>("MfaSecretKey")
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("Iterations")
                    .HasColumnType("int");

                b.Property<string>("PasswordHashAlgorithm")
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("FailedLoginCount")
                    .HasColumnType("int");

                b.Property<DateTime?>("LockoutEndUtc")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>("LastFailedLoginUtc")
                    .HasColumnType("datetime2");

                b.HasKey("Id");

                b.ToTable("Users");
            });
        }
    }
}
