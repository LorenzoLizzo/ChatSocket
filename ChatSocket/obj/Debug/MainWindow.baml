   M S B A M L   `   `   `  ����E  AChatSocket, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ChatSocket
ChatSocket  5   	       ChatSocket.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:ChatSocket     5   	   -   5          � Title$
  Chat��5   	   $	��450��6   $	��800��6#    � 
ResizeMode$ CanMinimize=�6H    � WindowStartupLocation$ CenterScreen=�6a   .��5	      � .+�5
   
   � -   5         �Name$ 	gridPorta��5
      $��Center=�6"   $��50q�6=    � ColumnDefinitions 5      �� 5      $
��auto �6#   �� 5      $��* �6#    � RowDefinitions 5      �� 5      $
��auto �6    �� 5      $��* �6    �� 5      $��* �6    �� 5      $
��auto �6    .+�5      �� $��0��6   $��2��6%   $G��AInserisci la porta a cui vuoi collegare la tua chat (49152-65535)��69   $��30(�6�   $
��Wrap=�6�   $��DemiBold"�6�   $��0,0,0,10q�6�   �� 5      $��1��6   $��0��6%   $��Nome Utente:��65   $��30(�6I   $��DemiBold"�6W   $��Center=�6m   $��0,0,0,10q�6�   �� 5      -   5         ��Name$ txtBoxNomeUtente��5   3   $��1��6   $��1��6#   $��30(�6M   $
��Bold"�6[   $��	10,0,0,10q�6m   �� 5      $��2��6   $��0��6%   $��Porta:��65   $��30(�6C   $��DemiBold"�6Q   $��Center=�6g   $��0,0,0,10q�6�   �� 5      -   5        $ txtBoxPorta��5   3   $��2��6   $��1��6#   $��30(�6H   $
��Bold"�6V   $��	10,0,0,10q�6h   �� 5      -   5         ��Name$ btnEntra��5   6   $��3��6   $��2��6"   $��Entra!��6H   $��30(�6Y   $��DemiBold"�6g   6}   5      � 5!   
   -   5        $ gridChat��5!      $y�	Collapsed=�6!    5"      �� 5#      $��* �6    �� 5$      $��* �6     5&      �� 5'      $��* �6#   �� 5(      $��* �6#   .+�5+      �� -   5         ��Name$ lstBoxMessaggi��5+      $��0��6.   $��2��6>   $��8q�6O   ]� 5-      $��1��6   $��0��6(   $��8q�65   .+�5.      �� $��Agenda��6   $	��Top=�6*   �� 5/      -   5        $ lstBoxAgenda��5/      60   50      �� 52      $��1��6   $��1��6)   $��8q�66   .+�53      �� $��0,0,0,10q�6   .+�54      �� $��Inserire messaggio��6    �� 55      -	   5        $ txtBoxMessaggio��55      	 �� AcceptsReturn	 . 
 �� TextWrapping$

 Wrap=�6L   $	��130��6`   56      �� 57       �� Orientation$ 
Horizontal=�6   $��Center=�66   .+�58      �� -
   5        $ btnInvia��58      $��Invia al singolo��6/   $��30��6J   $	��150��6V   $	��5,0q�6y   �� 59      -   5        $ btnInviaBroadcast��59      $��Invia in broadcast��68   $��30��6U   $	��150��6a   $	��5,0q�6m   6z   5:      5<      5=      5>      5@      