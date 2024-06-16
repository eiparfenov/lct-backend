﻿// <auto-generated />
using System;
using LctHack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LctHack.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240616141339_UpdateVideo")]
    partial class UpdateVideo
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LctHack.Models.Match", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("EndTime")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("end_time");

                    b.Property<string>("EndTimeMatch")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("end_time_match");

                    b.Property<string>("MatchFromTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("match_from_title");

                    b.Property<string>("StartTime")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("start_time");

                    b.Property<string>("StartTimeMatch")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("start_time_match");

                    b.Property<Guid>("VideoId")
                        .HasColumnType("uuid")
                        .HasColumnName("video_id");

                    b.HasKey("Id")
                        .HasName("pk_matches");

                    b.HasIndex("VideoId")
                        .HasDatabaseName("ix_matches_video_id");

                    b.ToTable("matches", (string)null);
                });

            modelBuilder.Entity("LctHack.Models.Video", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("FormTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("form_title");

                    b.Property<string>("MlId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("ml_id");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<int>("VideoState")
                        .HasColumnType("integer")
                        .HasColumnName("video_state");

                    b.HasKey("Id")
                        .HasName("pk_videos");

                    b.ToTable("videos", (string)null);
                });

            modelBuilder.Entity("LctHack.Models.Match", b =>
                {
                    b.HasOne("LctHack.Models.Video", "Video")
                        .WithMany()
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_matches_videos_video_id");

                    b.Navigation("Video");
                });
#pragma warning restore 612, 618
        }
    }
}