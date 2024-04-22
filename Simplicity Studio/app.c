/***************************************************************************//**
 * @file
 * @brief Core application logic.
 *******************************************************************************
 * # License
 * <b>Copyright 2020 Silicon Laboratories Inc. www.silabs.com</b>
 *******************************************************************************
 *
 * SPDX-License-Identifier: Zlib
 *
 * The licensor of this software is Silicon Laboratories Inc.
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 *
 ******************************************************************************/
#include "em_common.h"
#include "app_assert.h"
#include "sl_bluetooth.h"
#include "gatt_db.h"
#include "app.h"

//#include "retargetswo.h"
#include <stdio.h>


#define KEY_ARRAY_SIZE         25
#define MODIFIER_INDEX         0
#define DATA_INDEX             2

#define CAPSLOCK_KEY_OFF       0x00
#define CAPSLOCK_KEY_ON        0x02

// The advertising set handle allocated from Bluetooth stack.
static uint8_t advertising_set_handle = 0xff;
uint8_t hololensConnection;
uint8_t notification_data[8] = {0, 0, 0, 0, 0, 0, 0, 0};
uint16_t notification_len = 6;
uint8_t notifyEnabled = false;
uint8_t reportNotificationsEnabled = true;

uint8_t targetAddress[6] = {0x00, 0x46, 0xAB, 0x3F, 0x23, 0xAC};

static uint8_t counter = 0;

/**************************************************************************//**
 * Application Init.
 *****************************************************************************/
SL_WEAK void app_init(void)
{
  /////////////////////////////////////////////////////////////////////////////
  // Put your additional application init code here!                         //
  // This is called once during start-up.                                    //
  /////////////////////////////////////////////////////////////////////////////
  //RETARGET_SwoInit();
  printf("initializing...\n");
}

/**************************************************************************//**
 * Application Process Action.
 *****************************************************************************/
SL_WEAK void app_process_action(void)
{
  /////////////////////////////////////////////////////////////////////////////
  // Put your additional application code here!                              //
  // This is called infinitely.                                              //
  // Do not call blocking functions from here!                               //
  /////////////////////////////////////////////////////////////////////////////
  //printf("processing action.\n");
}

/**************************************************************************//**
 * Bluetooth stack event handler.
 * This overrides the dummy weak implementation.
 *
 * @param[in] evt Event coming from the Bluetooth stack.
 *****************************************************************************/

#define   sl_bt_evt_scanner_legacy_advertisement_report_id   0x000500a0
#define   sl_bt_evt_scanner_extended_advertisement_report_id   0x020500a0

void PrintAddress(bd_addr * address)
{
  printf("Address: %x:%x:%x:%x:%x:%x\n", address->addr[5], address->addr[4], address->addr[3], address->addr[2], address->addr[1], address->addr[0]);
}
void PrintAddressRaw(uint8_t * address)
{
  printf("Address: %x:%x:%x:%x:%x:%x\n", address[5], address[4], address[3], address[2], address[1], address[0]);
}
void HandleScanHit(bd_addr * address, int8_t rssi)
{
 notification_data[0] = address->addr[5];
 notification_data[1] = address->addr[4];
 notification_data[2] = address->addr[3];
 notification_data[3] = address->addr[2];
 notification_data[4] = address->addr[1];
 notification_data[5] = address->addr[0];
 notification_data[6] = rssi;
}

bool addressMatch(bd_addr * address)
{
  bool match = true;
  for( int i = 0; i < 6; i++)
    {
      if (address->addr[i] != targetAddress[i])
        {
          match = false;
        }
    }
  return match;
}
void setTargetAddress(uint8_t * newAddress)
{
  for( int i = 0; i < 6; i++) {
      targetAddress[i] = newAddress[5 - i];
  }
  printf("Setting New Target Address:\n");
  PrintAddressRaw(newAddress);
}

void sl_bt_on_event(sl_bt_msg_t *evt)
{
  sl_status_t sc;

  switch (SL_BT_MSG_ID(evt->header)) {
    // -------------------------------
    // This event indicates the device has started and the radio is ready.
    // Do not call any stack command before receiving this boot event!
    case sl_bt_evt_system_boot_id:
      printf("Bluetooth Stack Booted\n");
      sc = sl_bt_scanner_set_parameters(0, 1600, 1600);
      if (sc == SL_STATUS_OK)
        {
          printf("Successfully Created Scanner\n");
          sl_status_t scanner_status = sl_bt_scanner_start(0x5, 0x2);
          if (scanner_status == SL_STATUS_OK)
            {
              printf("Scanner Started.....\n");
            }
        }
      else {
          printf("Error creating scanner: %lu", sc);
      }

      printf("Setting up advertiser..\n");

      sc = sl_bt_advertiser_create_set(&advertising_set_handle);
      app_assert_status(sc);

      // Generate data for advertising
      sc = sl_bt_legacy_advertiser_generate_data(advertising_set_handle,
                                                 sl_bt_advertiser_general_discoverable);
      app_assert_status(sc);

      // Set advertising interval to 100ms.
      sc = sl_bt_advertiser_set_timing(
        advertising_set_handle,
        160, // min. adv. interval (milliseconds * 1.6)
        160, // max. adv. interval (milliseconds * 1.6)
        0,   // adv. duration
        0);  // max. num. adv. events
      app_assert_status(sc);
      // Start advertising and enable connections.
      sc = sl_bt_sm_configure(0, sl_bt_sm_io_capability_noinputnooutput);
      app_assert_status(sc);
      sc = sl_bt_sm_set_bondable_mode(1);
      app_assert_status(sc);
      sc = sl_bt_legacy_advertiser_start(advertising_set_handle,
                                         sl_bt_advertiser_connectable_scannable);
      app_assert_status(sc);

      break;

    case sl_bt_evt_scanner_legacy_advertisement_report_id:
      //printf("---GOT LEGACY ADVERTISEMENT---\n");
      bd_addr address = evt->data.evt_scanner_legacy_advertisement_report.address;
      if (addressMatch(&address))
        {
          PrintAddress(&address);
          int8_t rssi = evt->data.evt_scanner_legacy_advertisement_report.rssi;
          printf("RSSI:\t%i\n\n", rssi);
          HandleScanHit(&address, rssi);
          sl_bt_external_signal(1);
          sl_bt_external_signal(2);
        }

      //printf("Data:%i", data);
      break;

    case sl_bt_evt_scanner_extended_advertisement_report_id:
      printf("GOT EXTENDED ADVERTISEMENT.\n");
      break;

     //CONNECTION HAPPENED
    case sl_bt_evt_connection_opened_id:
      hololensConnection = evt->data.evt_connection_opened.connection;
      printf("Bluetooth Connection Happened...\n");
      sc = sl_bt_sm_increase_security(evt->data.evt_connection_opened.connection);
      app_assert_status(sc);

      //notifyEnabled = true;
      break;

    // -------------------------------
    // This event indicates that a connection was closed.
    case sl_bt_evt_connection_closed_id:
      printf("Bluetooth connection closed.\n");
      // Generate data for advertising
      sc = sl_bt_legacy_advertiser_generate_data(advertising_set_handle,
                                                 sl_bt_advertiser_connectable_scannable);
      app_assert_status(sc);

      // Restart advertising after client has disconnected.
      sc = sl_bt_legacy_advertiser_start(advertising_set_handle,
                                         sl_bt_legacy_advertiser_connectable);
      app_assert_status(sc);
      notifyEnabled = false;
      break;
    case sl_bt_evt_gatt_server_user_write_request_id:
      printf("Handling Write request...\n");
      if (evt->data.evt_gatt_server_user_write_request.characteristic == gattdb_TARGET_ADDRESS) {
          setTargetAddress(evt->data.evt_gatt_server_attribute_value.value.data);

          //send an OK write response
          sl_bt_gatt_server_send_user_write_response(
                     evt->data.evt_gatt_server_user_write_request.connection,
                     gattdb_TARGET_ADDRESS, SL_STATUS_OK);
      }
    break;


    case sl_bt_evt_sm_bonded_id:
          printf("successful bonding\r\n");
          break;

    case sl_bt_evt_sm_bonding_failed_id:
          printf("bonding failed, reason 0x%2X\r\n",
                  evt->data.evt_sm_bonding_failed.reason);

          sc = sl_bt_sm_delete_bondings();
          app_assert_status(sc);
          sc = sl_bt_connection_close(evt->data.evt_sm_bonding_failed.connection);
          app_assert_status(sc);
          break;

    case sl_bt_evt_system_external_signal_id:
      if (notifyEnabled)
        {
          if (evt->data.evt_system_external_signal.extsignals == 1)
            {
              printf("Sending Notification %u\n", counter);
              notification_data[7] = counter;
              counter++;

              sc = sl_bt_gatt_server_send_notification(hololensConnection, gattdb_RSSI_VALUE, 8, &notification_data);
            }
        }
/*
      if (reportNotificationsEnabled == 1 && evt->data.evt_system_external_signal.extsignals == 2) {
              memset(input_report_data, 0, sizeof(input_report_data));

              if (KEY_ARRAY_SIZE == counter) {
                      counter = 0;
                    } else {
                      counter++;
                    }
              actual_key = reduced_key_array[counter];
              input_report_data[MODIFIER_INDEX] = CAPSLOCK_KEY_OFF;
              input_report_data[DATA_INDEX] = actual_key;

              sc = sl_bt_gatt_server_notify_all(gattdb_report,
                                                sizeof(input_report_data),
                                                input_report_data);
              app_assert_status(sc);

              printf("Key report was sent. Sent:\t%u\r\n", input_report_data[DATA_INDEX]);

            }*/
      break;
    case  sl_bt_evt_gatt_server_characteristic_status_id:

      if ((evt->data.evt_gatt_server_characteristic_status.characteristic == gattdb_RSSI_VALUE)
          && (evt->data.evt_gatt_server_characteristic_status.status_flags == 0x01)) {
       if (evt->data.evt_gatt_server_characteristic_status.client_config_flags == 0x00) {
             notifyEnabled = false;
       }
       else {
          printf("Enabling Notifications...\n");
          notifyEnabled = true;
       }
    }

      if (evt->data.evt_gatt_server_characteristic_status.characteristic
                == gattdb_report) {
              // client characteristic configuration changed by remote GATT client
              if (evt->data.evt_gatt_server_characteristic_status.status_flags
                  == sl_bt_gatt_server_client_config) {
                if (evt->data.evt_gatt_server_characteristic_status.
                    client_config_flags == sl_bt_gatt_server_notification) {
                    printf("Enabling Report Notifications...\n");
                    reportNotificationsEnabled = 1;
                } else {
                    reportNotificationsEnabled = 0;
                }
              }
            }


    break;
    default:
      //printf(evt->header);
      //printf("\n");
      break;
  }
}
