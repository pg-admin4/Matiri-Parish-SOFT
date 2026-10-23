using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MatiriParish.Core.Migrations
{
    [DbContext(typeof(MatiriParish.Core.Data.MatiriParishDbContext))]
    public partial class MatiriParishDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("MatiriParish.Core.Models.User", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("PasswordSalt")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<int>("Iterations")
                    .HasColumnType("integer");

                b.Property<string>("PasswordHashAlgorithm")
                    .HasColumnType("text");

                b.Property<bool>("IsMfaEnabled")
                    .HasColumnType("boolean");

                b.Property<int>("Role")
                    .HasColumnType("integer");

                b.Property<int>("FailedLoginCount")
                    .HasColumnType("integer");

                b.Property<DateTime?>("LockoutEndUtc")
                    .HasColumnType("timestamp without time zone");

                b.Property<DateTime?>("LastFailedLoginUtc")
                    .HasColumnType("timestamp without time zone");

                b.HasKey("Id");

                b.ToTable("Users");
            });
        }
    }
}
