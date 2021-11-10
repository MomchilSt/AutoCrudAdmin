﻿// <auto-generated />
using System;
using AutoCrudAdmin.Demo.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AutoCrudAdmin.Demo.SqlServer.Migrations
{
    [DbContext(typeof(TaskSystemDbContext))]
    [Migration("20211110084611_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Phone")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.EmployeeTasks", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("TaskId");

                    b.ToTable("EmployeeTasks");
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OpenDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Projects");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DueDate = new DateTime(2022, 1, 1, 12, 3, 25, 0, DateTimeKind.Unspecified),
                            Name = "Setup migration to PostgreSQL",
                            OpenDate = new DateTime(2021, 11, 10, 10, 46, 11, 44, DateTimeKind.Local).AddTicks(2230)
                        },
                        new
                        {
                            Id = 2,
                            DueDate = new DateTime(2021, 10, 4, 12, 3, 25, 0, DateTimeKind.Unspecified),
                            Name = "Update packages",
                            OpenDate = new DateTime(2021, 11, 10, 10, 46, 11, 51, DateTimeKind.Local).AddTicks(3770)
                        },
                        new
                        {
                            Id = 3,
                            DueDate = new DateTime(2021, 11, 8, 12, 3, 25, 0, DateTimeKind.Unspecified),
                            Name = "Integrate AutoCrudAdmin",
                            OpenDate = new DateTime(2021, 11, 10, 10, 46, 11, 51, DateTimeKind.Local).AddTicks(3940)
                        });
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.Task", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ExecutionType")
                        .HasColumnType("int");

                    b.Property<int>("LabelType")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime>("OpenDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Tasks");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DueDate = new DateTime(2021, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ExecutionType = 4,
                            LabelType = 0,
                            Name = "Check incompatible entities",
                            OpenDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ProjectId = 1
                        },
                        new
                        {
                            Id = 12,
                            DueDate = new DateTime(2021, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ExecutionType = 3,
                            LabelType = 0,
                            Name = "Change Db connection",
                            OpenDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ProjectId = 1
                        },
                        new
                        {
                            Id = 3,
                            DueDate = new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ExecutionType = 1,
                            LabelType = 0,
                            Name = "Setup PostgreSQL server",
                            OpenDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ProjectId = 1
                        },
                        new
                        {
                            Id = 4,
                            DueDate = new DateTime(2021, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ExecutionType = 4,
                            LabelType = 0,
                            Name = "Update all packages",
                            OpenDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ProjectId = 2
                        },
                        new
                        {
                            Id = 5,
                            DueDate = new DateTime(2021, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ExecutionType = 3,
                            LabelType = 0,
                            Name = "Install AutoCrudAdmin",
                            OpenDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ProjectId = 3
                        },
                        new
                        {
                            Id = 6,
                            DueDate = new DateTime(2021, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ExecutionType = 3,
                            LabelType = 0,
                            Name = "Setup AutoCrudAdmin",
                            OpenDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            ProjectId = 3
                        });
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.EmployeeTasks", b =>
                {
                    b.HasOne("AutoCrudAdmin.Demo.Models.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AutoCrudAdmin.Demo.Models.Task", "Task")
                        .WithMany("EmployeeTasks")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Task");
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.Task", b =>
                {
                    b.HasOne("AutoCrudAdmin.Demo.Models.Project", "Project")
                        .WithMany("Tasks")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.Project", b =>
                {
                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("AutoCrudAdmin.Demo.Models.Task", b =>
                {
                    b.Navigation("EmployeeTasks");
                });
#pragma warning restore 612, 618
        }
    }
}
