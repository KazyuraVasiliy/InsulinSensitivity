﻿// <auto-generated />
using System;
using DataAccessLayer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataAccessLayer.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20220702080453_Nightscout")]
    partial class Nightscout
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.10");

            modelBuilder.Entity("DataAccessLayer.Models.Eating", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("AccuracyAuto")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AccuracyUser")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("ActiveInsulinEnd")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("ActiveInsulinStart")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("BasalDose")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("BasalInjectionTime")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("BasalRate")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("BasalRateCoefficient")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("BasalTypeId")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("BolusDoseCalculate")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("BolusDoseFact")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("BolusDoseTotal")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("BolusTypeId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Carbohydrate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EatingTypeId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EndEating")
                        .HasColumnType("TEXT");

                    b.Property<string>("Error")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ExerciseId")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("ExpectedGlucose")
                        .HasColumnType("TEXT");

                    b.Property<int>("Fat")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ForecastError")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("GlucoseEnd")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("GlucoseStart")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("InjectionTime")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("InsulinSensitivityAutoFour")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("InsulinSensitivityAutoOne")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("InsulinSensitivityAutoThree")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("InsulinSensitivityAutoTwo")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("InsulinSensitivityFact")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("InsulinSensitivityUser")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBatteryReplacement")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsCannulaReplacement")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsCartridgeReplacement")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsCatheterReplacement")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsIgnored")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsMenstrualCycleStart")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsMonitoringReplacement")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Pause")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Protein")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("WorkingTime")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("WriteOff")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BasalTypeId");

                    b.HasIndex("BolusTypeId");

                    b.HasIndex("EatingTypeId");

                    b.HasIndex("ExerciseId");

                    b.HasIndex("UserId");

                    b.ToTable("Eatings");
                });

            modelBuilder.Entity("DataAccessLayer.Models.EatingType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBasal")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("TimeEnd")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("TimeStart")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("EatingTypes");
                });

            modelBuilder.Entity("DataAccessLayer.Models.Exercise", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ExerciseTypeId")
                        .HasColumnType("TEXT");

                    b.Property<int>("HoursAfterInjection")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ExerciseTypeId");

                    b.ToTable("Exercises");
                });

            modelBuilder.Entity("DataAccessLayer.Models.ExerciseType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("DateDeleted")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEmpty")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ExerciseTypes");
                });

            modelBuilder.Entity("DataAccessLayer.Models.ExpendableMaterial", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("ChangeType")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Count")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<int>("ExpendableMaterialTypeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ExpendableMaterialTypeId");

                    b.ToTable("ExpendableMaterials");
                });

            modelBuilder.Entity("DataAccessLayer.Models.ExpendableMaterialType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Unit")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ExpendableMaterialTypes");
                });

            modelBuilder.Entity("DataAccessLayer.Models.Injection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("BolusDose")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("BolusTypeId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EatingId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("InjectionDate")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("InjectionTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BolusTypeId");

                    b.HasIndex("EatingId");

                    b.ToTable("Injections");
                });

            modelBuilder.Entity("DataAccessLayer.Models.InsulinType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBasal")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Offset")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("InsulinTypes");
                });

            modelBuilder.Entity("DataAccessLayer.Models.IntermediateDimension", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DimensionDate")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("DimensionTime")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EatingId")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Glucose")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EatingId");

                    b.ToTable("IntermediateDimensions");
                });

            modelBuilder.Entity("DataAccessLayer.Models.MenstrualCycle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateStart")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("MenstrualCycles");
                });

            modelBuilder.Entity("DataAccessLayer.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("AbsorptionRateOfCarbohydrates")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("AbsorptionRateOfFats")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("AbsorptionRateOfProteins")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BasalTypeId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BolusTypeId")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("CarbohydrateCoefficient")
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("DosingAccuracy")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("FatCoefficient")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Gender")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("HighSugar")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Hyperglycemia")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Hypoglycemia")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsMonitoring")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsPump")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("LowSugar")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("NightscoutUri")
                        .HasColumnType("TEXT");

                    b.Property<int>("PeriodOfCalculation")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("ProteinCoefficient")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetGlucose")
                        .HasColumnType("TEXT");

                    b.Property<int>("Weight")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BasalTypeId");

                    b.HasIndex("BolusTypeId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DataAccessLayer.Models.Eating", b =>
                {
                    b.HasOne("DataAccessLayer.Models.InsulinType", "BasalType")
                        .WithMany("BasalEatings")
                        .HasForeignKey("BasalTypeId");

                    b.HasOne("DataAccessLayer.Models.InsulinType", "BolusType")
                        .WithMany("BolusEatings")
                        .HasForeignKey("BolusTypeId");

                    b.HasOne("DataAccessLayer.Models.EatingType", "EatingType")
                        .WithMany("Eatings")
                        .HasForeignKey("EatingTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataAccessLayer.Models.Exercise", "Exercise")
                        .WithMany("Eatings")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataAccessLayer.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataAccessLayer.Models.Exercise", b =>
                {
                    b.HasOne("DataAccessLayer.Models.ExerciseType", "ExerciseType")
                        .WithMany("Exercises")
                        .HasForeignKey("ExerciseTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataAccessLayer.Models.ExpendableMaterial", b =>
                {
                    b.HasOne("DataAccessLayer.Models.ExpendableMaterialType", "ExpendableMaterialType")
                        .WithMany("ExpendableMaterials")
                        .HasForeignKey("ExpendableMaterialTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataAccessLayer.Models.Injection", b =>
                {
                    b.HasOne("DataAccessLayer.Models.InsulinType", "BolusType")
                        .WithMany("BolusInjections")
                        .HasForeignKey("BolusTypeId");

                    b.HasOne("DataAccessLayer.Models.Eating", "Eating")
                        .WithMany("Injections")
                        .HasForeignKey("EatingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataAccessLayer.Models.IntermediateDimension", b =>
                {
                    b.HasOne("DataAccessLayer.Models.Eating", "Eating")
                        .WithMany("IntermediateDimensions")
                        .HasForeignKey("EatingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataAccessLayer.Models.MenstrualCycle", b =>
                {
                    b.HasOne("DataAccessLayer.Models.User", "User")
                        .WithMany("MenstrualCycles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataAccessLayer.Models.User", b =>
                {
                    b.HasOne("DataAccessLayer.Models.InsulinType", "BasalType")
                        .WithMany("BasalUsers")
                        .HasForeignKey("BasalTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataAccessLayer.Models.InsulinType", "BolusType")
                        .WithMany("BolusUsers")
                        .HasForeignKey("BolusTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
