﻿// <auto-generated />
using System;
using LibraryServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LibraryServer.Migrations
{
    [DbContext(typeof(ImagesLibraryContext))]
    partial class ImagesLibraryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("LibraryServer.ImageRecognized", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ImageName")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ImageRecognizedDetailsId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ImageTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ImageRecognizedDetailsId");

                    b.HasIndex("ImageTypeId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("LibraryServer.ImageRecognizedDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("BinaryFile")
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Details");
                });

            modelBuilder.Entity("LibraryServer.PredictionResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PredictionStringResult")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TypesOfImages");
                });

            modelBuilder.Entity("LibraryServer.ImageRecognized", b =>
                {
                    b.HasOne("LibraryServer.ImageRecognizedDetails", "ImageRecognizedDetails")
                        .WithMany()
                        .HasForeignKey("ImageRecognizedDetailsId");

                    b.HasOne("LibraryServer.PredictionResult", "ImageType")
                        .WithMany("RecognizedImages")
                        .HasForeignKey("ImageTypeId");

                    b.Navigation("ImageRecognizedDetails");

                    b.Navigation("ImageType");
                });

            modelBuilder.Entity("LibraryServer.PredictionResult", b =>
                {
                    b.Navigation("RecognizedImages");
                });
#pragma warning restore 612, 618
        }
    }
}
