import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';

interface WithdrawalItem {
  articleNumber: string;
  name: string;
  quantity: number;
}

interface EmployeeWithdrawalData {
  employeeName: string;
  items: WithdrawalItem[];
}

@Component({
  selector: 'app-employee-withdrawal',
  imports: [CommonModule],
  templateUrl: './employee-withdrawal.html',
  styleUrl: './employee-withdrawal.css',
})
export class EmployeeWithdrawal implements OnInit {
  data: EmployeeWithdrawalData = {
    employeeName: '',
    items: []
  };
  currentDate = new Date();

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['data']) {
        try {
          this.data = JSON.parse(decodeURIComponent(params['data']));
        } catch (e) {
          console.error('Error parsing data:', e);
        }
      }
    });
  }
}
