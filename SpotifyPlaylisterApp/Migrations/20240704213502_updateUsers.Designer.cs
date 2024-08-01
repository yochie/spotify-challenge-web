﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpotifyPlaylisterApp.Data;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    [DbContext(typeof(SpotifyPlaylisterAppContext))]
    [Migration("20240704213502_updateUsers")]
    partial class updateUsers
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("SpotifyUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SpotifyUserId");

                    b.ToTable("Playlist");
                });

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.PlaylistTrack", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Album")
                        .HasColumnType("TEXT");

                    b.Property<string>("Artists")
                        .HasColumnType("TEXT");

                    b.Property<int>("PlaylistId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistId");

                    b.ToTable("PlaylistTrack");
                });

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.SpotifyUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SpotifyUser");
                });

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.Playlist", b =>
                {
                    b.HasOne("SpotifyPlaylisterApp.Models.SpotifyUser", "SpotifyUser")
                        .WithMany("Playlists")
                        .HasForeignKey("SpotifyUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SpotifyUser");
                });

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.PlaylistTrack", b =>
                {
                    b.HasOne("SpotifyPlaylisterApp.Models.Playlist", "Playlist")
                        .WithMany("Tracks")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Playlist");
                });

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.Playlist", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("SpotifyPlaylisterApp.Models.SpotifyUser", b =>
                {
                    b.Navigation("Playlists");
                });
#pragma warning restore 612, 618
        }
    }
}
